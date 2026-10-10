namespace MbyteWeb.Server.ViewModels
{
    public class QueryResponse<T>
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public required IEnumerable<T> Result { get; set; }
    }
}
