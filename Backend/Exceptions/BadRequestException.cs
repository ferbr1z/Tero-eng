namespace MbyteWeb.Server.Exceptions
{
    public class BadRequestException : ArgumentException
    {
        public BadRequestException() : base("Parámetros inválidos")
        { }

        public BadRequestException(string message) : base(message)
        { }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
