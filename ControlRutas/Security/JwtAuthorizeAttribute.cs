using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;

namespace ControlRutas.Security
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authHeader = httpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring(7); // Extrae el token del encabezado
                try
                {
                    // Aquí necesitas validar el token manualmente
                    // Utiliza la librería de JWT para verificar el token basado en tu 'secret'
                    var valid = ValidateToken(token);
                    return valid;
                }
                catch (Exception)
                {
                    // Manejar la excepción o fallo en la validación del token
                    return false;
                }
            }
            return false;
        }

        private bool ValidateToken(string token)
        {
            // Decodificar la clave secreta desde la configuración, asumiendo que ya está en base64
            var secret = ConfigurationManager.AppSettings["config:JwtKey"];
            var mySecretBytes = TextEncodings.Base64Url.Decode(secret);
            var mySecurityKey = new SymmetricSecurityKey(mySecretBytes);

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = ConfigurationManager.AppSettings["config:JwtIssuer"],
                    ValidAudience = ConfigurationManager.AppSettings["config:JwtAudience"],
                    IssuerSigningKey = mySecurityKey,
                    ValidateLifetime = true, // Asegura que el token no esté expirado
                    ClockSkew = TimeSpan.Zero, // Reduce la posibilidad de problemas relacionados con la sincronización de tiempo
                    SignatureValidator = (t, parameters) =>
                    {
                        var jwt = new JwtSecurityToken(t);
                        // Si necesitas realizar comprobaciones adicionales del token, puedes hacerlo aquí
                        return jwt;
                    }
                }, out SecurityToken validatedToken);

                return true; // El token es válido
            }
            catch (Exception ex)
            {
                // Puedes agregar un registro de excepciones si es necesario para depurar
                System.Diagnostics.Debug.WriteLine("Error validating token: " + ex.Message);
                return false; // Hubo un fallo validando el token
            }
        }

    }
}
