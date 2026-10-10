namespace MbyteWeb.Server.ViewModels.Auth
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool IsTemporal { get; set; } = false;
        public IEnumerable<string> Permisos { get; set; } = [];
    }
}
