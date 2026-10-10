using System.Security.Claims;

namespace MbyteWeb.Server.Services.Interfaces.Auth
{
    public interface IJwtService
    {
        public string GenerateToken(List<Claim> claims);
        public string GenerateToken(List<Claim> claims, DateTime expirationDate);
    }
}
