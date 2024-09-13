using Microsoft.Owin;
using Owin;
using System.Configuration;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.IdentityModel.Logging;

[assembly: OwinStartupAttribute(typeof(ControlRutas.Startup))]
namespace ControlRutas
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureOAuthTokenGeneration(app);
        }

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["config:JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["config:JwtAudience"];
            var secret = ConfigurationManager.AppSettings["config:JwtKey"];

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { audience },
                IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
                {
                    new SymmetricKeyIssuerSecurityKeyProvider(issuer, TextEncodings.Base64Url.Decode(secret))
                }
            });

            IdentityModelEventSource.ShowPII = true;
        }
    }
}
