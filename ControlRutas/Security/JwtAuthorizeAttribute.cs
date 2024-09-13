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
                    // Aqu� necesitas validar el token manualmente
                    // Utiliza la librer�a de JWT para verificar el token basado en tu 'secret'
                    var valid = ValidateToken(token);
                    return valid;
                }
                catch (Exception)
                {
                    // Manejar la excepci�n o fallo en la validaci�n del token
                    return false;
                }
            }
            return false;
        }

        private bool ValidateToken(string token)
        {
            // Decodificar la clave secreta desde la configuraci�n, asumiendo que ya est� en base64
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
                    ValidateLifetime = true, // Asegura que el token no est� expirado
                    ClockSkew = TimeSpan.Zero, // Reduce la posibilidad de problemas relacionados con la sincronizaci�n de tiempo
                    SignatureValidator = (t, parameters) =>
                    {
                        var jwt = new JwtSecurityToken(t);
                        // Si necesitas realizar comprobaciones adicionales del token, puedes hacerlo aqu�
                        return jwt;
                    }
                }, out SecurityToken validatedToken);

                return true; // El token es v�lido
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
