﻿using ControlRutas.Data;
using ControlRutas.DTO;
using ControlRutas.Helpers;
using ControlRutas.Models;
using ControlRutas.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace ControlRutas.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly FirebaseService _firebaseService;

        public AccountController()
        {
            _firebaseService = new FirebaseService();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult NuevoUsuario()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NuevoUsuario(RegisterViewModel model)
        {
            ControlRutasEntities db = new ControlRutasEntities();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                Usuarios usuarios = new Usuarios()
                {
                    GUID = user.Id,
                    Email = model.Email,
                    NumeroTelefono = model.NumeroTelefono,
                    PrimerNombre = model.PrimerNombre,
                    SegundoNombre = model.SegundoNombre,
                    PrimerApellido = model.PrimerApellido,
                    SegundoApellido = model.SegundoApellido,
                    IdTipoUsuario = model.id_tipo_usuario,
                    Estado = 1,
                    MessageToken = ""
                };

                
                var result = await UserManager.CreateAsync(user, model.Password);

                db.Usuarios.Add(usuarios);
                UsuariosEstablecimientos usuariosEstablecimientos = new UsuariosEstablecimientos()
                {
                    IdEstablecimientoEducativo = model.idEstablecimiento,
                    IdUsuario = usuarios.Id,
                    GUID = Guid.NewGuid().ToString(),
                    Activo = true
                };
                db.UsuariosEstablecimientos.Add(usuariosEstablecimientos);
                if (result.Succeeded)
                {
                    var response = _firebaseService.RegisterUserAsync(model.Email, model.Password, user.Id);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return View(model);
                    }

                    // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                    // Enviar un correo electrónico con este vínculo
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirmar la cuenta", "Para confirmar su cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                    return RedirectToAction("Index", "Usuarios");
                }
                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar un correo electrónico con este vínculo
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        // ...

        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> ForgotPasswordJSON(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                string token = CompressToken(code);

                using (var message = new MailMessage())
                {
                    message.To.Add(model.Email);
                    message.Subject = "Password Reset";
                    message.Body = $"A continuación se muestra el token para restablecer la contraseña: {token}";

                    using (var smtpClient = new SmtpClient("smtp.gmail.com", 587)) // Ajusta host y puerto
                    {
                        smtpClient.Credentials = new System.Net.NetworkCredential("marito.kun1@gmail.com", "jxiy sxdq tphg uxfx");
                        smtpClient.EnableSsl = true; // Habilitar SSL

                        await smtpClient.SendMailAsync(message);
                    }
                }

                return Json(new { success = true, message = "Correo Enviado" });
            }

            return Json(new { success = false, message = "Invalid model state" });
        }

        public static string CompressToken(string token)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(tokenBytes, 0, tokenBytes.Length);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static string DecompressToken(string compressedToken)
        {
            byte[] compressedTokenBytes = Convert.FromBase64String(compressedToken);
            using (MemoryStream memoryStream = new MemoryStream(compressedTokenBytes))
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        return Encoding.UTF8.GetString(resultStream.ToArray());
                    }
                }
            }
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> ResetPasswordJson(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, DecompressToken(model.Code), model.Password);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Password reset" });
            }
            AddErrors(result);
            return Json(new { success = false, message = "Error resetting password" });
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Aplicaciones auxiliares
        // Se usa para la protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        //
        // POST: /Account/LoginJSON
        // Login with JSON response
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> LoginJSON(LoginViewModel model)
        {
            // 'this' se usa implícitamente para acceder a miembros como ModelState, SignInManager, Json, db.
            // El alias 'accountController' no es necesario.

            model.RememberMe = false; // Lógica original mantenida

            if (!ModelState.IsValid) // Acceso directo a ModelState
            {
                return Json(new // Cast a (object) innecesario
                {
                    success = false,
                    message = "El modelo de datos no es válido." // "Invalid model state"
                }, JsonRequestBehavior.AllowGet); // Mantenido de tu original (0 es AllowGet)
            }

            // Usar SignInManager directamente. El resultado es un enum SignInStatus.
            // El parámetro 'lockoutOnFailure' se mantiene como 'false' según tu código.
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            switch (result)
            {
                case SignInStatus.Success:
                    // ADVERTENCIA: Revisa la nota sobre la instanciación de DbContext aquí.
                    using (ControlRutasEntities dbContext = new ControlRutasEntities())
                    {
                        // GenerateJWTAuth es estático en AccountController o un método de instancia.
                        // Si es estático: AccountController.GenerateJWTAuth(...)
                        // Si es de instancia: this.GenerateJWTAuth(...) o solo GenerateJWTAuth(...)
                        string jwtAuth = AccountController.GenerateJWTAuth(model.Email); // Asumiendo que es estático como en el original

                        // Obtener el usuario de la base de datos local de forma asíncrona
                        Usuarios usuarioEntity = await dbContext.Usuarios
                            .FirstOrDefaultAsync(u => u.Email == model.Email);

                        if (usuarioEntity == null)
                        {
                            // Esto significaría que ASP.NET Identity autenticó al usuario,
                            // pero no existe un registro correspondiente en tu tabla 'Usuarios'.
                            // Podría ser una inconsistencia de datos.
                            return Json(new
                            {
                                success = false,
                                message = "Usuario autenticado pero no encontrado en el sistema local."
                            }, JsonRequestBehavior.AllowGet);
                        }

                        // Obtener el UsuarioOB.
                        // NOTA: UsuarioActual.ObtenerUsuario crea su propio DbContext internamente según la limpieza anterior.
                        // Para optimizar, considera refactorizar ObtenerUsuario para que acepte un DbContext.
                        UsuarioOB usuarioOb = UsuarioActual.ObtenerUsuario(usuarioEntity.GUID);
                        // Si ObtenerUsuario fuera refactorizado:
                        // UsuarioOB usuarioOb = UsuarioActual.ObtenerUsuario(usuarioEntity.GUID, dbContext);

                        if (usuarioOb == null)
                        {
                            // Si ObtenerUsuario devuelve null (ej. tipo de usuario no manejado en su switch interno)
                            return Json(new
                            {
                                success = false,
                                message = "No se pudo obtener la información detallada del usuario tras el login."
                            }, JsonRequestBehavior.AllowGet);
                        }


                        return Json(new
                        {
                            success = true,
                            message = "Inicio de sesión exitoso.", // "Success"
                            token = jwtAuth,
                            usuario = usuarioOb
                        }, JsonRequestBehavior.AllowGet);
                    }

                case SignInStatus.LockedOut:
                    return Json(new
                    {
                        success = false,
                        message = "Cuenta bloqueada." // "Locked out"
                    }, JsonRequestBehavior.AllowGet);

                case SignInStatus.RequiresVerification:
                    return Json(new
                    {
                        success = false,
                        message = "Requiere verificación." // "Requires verification"
                    }, JsonRequestBehavior.AllowGet);

                case SignInStatus.Failure: // Cubre otros fallos
                default: // Por si acaso hay otros estados o para el caso de fallo
                    return Json(new
                    {
                        success = false,
                        message = "Intento de inicio de sesión inválido." // "Invalid login attempt"
                    }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> RegisterJSON(RegisterViewModel model)
        {
            ControlRutasEntities db = new ControlRutasEntities();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                Usuarios usuarios = new Usuarios()
                {
                    GUID = user.Id,
                    Email = model.Email,
                    NumeroTelefono = model.NumeroTelefono,
                    PrimerNombre = model.PrimerNombre,
                    SegundoNombre = model.SegundoNombre,
                    PrimerApellido = model.PrimerApellido,
                    SegundoApellido = model.SegundoApellido,
                    IdTipoUsuario = model.id_tipo_usuario,
                    Estado = 1
                };

                UsuariosEstablecimientos usuariosEstablecimientos = new UsuariosEstablecimientos()
                {
                    IdEstablecimientoEducativo = model.idEstablecimiento,
                    IdUsuario = usuarios.Id,
                    GUID = Guid.NewGuid().ToString(),
                    Activo = true
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                db.Usuarios.Add(usuarios);
                db.UsuariosEstablecimientos.Add(usuariosEstablecimientos);
                if (result.Succeeded)
                {
                    var response = _firebaseService.RegisterUserAsync(model.Email, model.Password, user.Id);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { success = false, message = "Error saving user - " + e.Message });
                    }

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    return Json(new { success = true, message = "Success" });
                }
                AddErrors(result);
            }

            return Json(new { success = false, message = "Invalid model state" });
        }

        public static string GenerateJWTAuth(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtHeaderParameterNames.Jku, email),
                new Claim(JwtHeaderParameterNames.Kid, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, email)
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"])));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddDays(Convert.ToDouble(Convert.ToString(ConfigurationManager.AppSettings["config:JwtExpireDays"])));

            var token = new JwtSecurityToken(
                Convert.ToString(ConfigurationManager.AppSettings["config:JwtIssuer"]),
                Convert.ToString(ConfigurationManager.AppSettings["config:JwtAudience"]),
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"]));
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Corrected access to the validatedToken
                var jwtToken = (JwtSecurityToken)validatedToken;
                var jku = jwtToken.Claims.First(claim => claim.Type == "jku").Value;
                var userName = jwtToken.Claims.First(claim => claim.Type == "kid").Value;

                return userName;
            }
            catch
            {
                return null;
            }
        }
    }
}