namespace MbyteWeb.Server.ViewModels
{
    public abstract class BaseQuery
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
