using MbyteWeb.Server.Services.Interfaces.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MbyteWeb.Server.Services.Implementations.Auth
{
    public class JwtService(IConfiguration configuration) : IJwtService
    {
        public string GenerateToken(List<Claim> claims, DateTime expirationDate)
        {
            var jwtConfig = configuration.GetSection("JwtSettings");

            var secretKey = jwtConfig.GetValue<string>("SecretKey");
            if (secretKey == null) throw new InvalidConfigurationException("The secret key was not found.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationDate,
                SigningCredentials = creds,
                Issuer = jwtConfig.GetValue<string>("Issuer"),
                Audience = jwtConfig.GetValue<string>("Audience")
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateToken(List<Claim> claims)
        {
            return GenerateToken(claims, DateTime.Now.AddDays(7));
        }
    }
}
