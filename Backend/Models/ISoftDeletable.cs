namespace MbyteWeb.Server.Models
{
    public interface ISoftDeletable
    {
        public bool Borrado { get; set; }
        public bool Protegido { get; set; }
    }
}
