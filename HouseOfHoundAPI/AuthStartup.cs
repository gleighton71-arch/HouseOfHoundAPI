using System;
using System.Configuration;
using System.Text;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Microsoft.IdentityModel.Tokens;
using Owin;

namespace HouseOfHoundAPI
{
    public static class AuthStartup
    {
        public static void ConfigureAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];
            var secret = ConfigurationManager.AppSettings["JwtSecret"];

            if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
                throw new Exception("JwtSecret must be at least 32 characters.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            // Tell Web API / OWIN: use JWT bearer tokens to authenticate requests
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2) // small allowance for clock drift
                }
            });
        }
    }
}