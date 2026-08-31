using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService
{
    public string CreateToken(string userId, string email, string[] roles, bool mustChangePassword)
    {
        var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
        var audience = ConfigurationManager.AppSettings["JwtAudience"];
        var secret = ConfigurationManager.AppSettings["JwtSecret"];
        var expiryHours = int.Parse(ConfigurationManager.AppSettings["JwtExpiryHours"]);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddHours(expiryHours);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, email ?? ""),
            new Claim(ClaimTypes.Email, email ?? ""),
            new Claim("mustChangePassword", mustChangePassword ? "true" : "false")
        };

        var identity = new ClaimsIdentity(claims, "Bearer");

        // add role claims
        if (roles != null)
        {
            foreach (var r in roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, r));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: identity.Claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}