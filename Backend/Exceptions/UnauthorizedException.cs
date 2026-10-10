namespace MbyteWeb.Server.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(): base("No autenticado")
        { }

        public UnauthorizedException(string message) : base(message)
        { }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
