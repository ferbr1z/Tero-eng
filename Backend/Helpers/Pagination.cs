using MbyteWeb.Server.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Helpers
{
    public static class QueryResponseBuilder
    {
        public async static Task<QueryResponse<R>> Create<S, R>(IQueryable<S> source, int? queryPage, int? queryPageSize, Func<S, R> mapFunction)
        {
            int pageSize = queryPageSize ?? -1;
            int count = await source.CountAsync();
            int page = -1;

            if (pageSize > 0)
            {
                int pageCount = getPageCount(count, pageSize);
                page = Math.Max(Math.Min(queryPage ?? 0, pageCount - 1), 0);
                source = source.Skip(page * pageSize).Take(pageSize);
            }

            var items = source.Select(mapFunction);

            return new QueryResponse<R>
            {
                Count = count,
                Page = page,
                PageSize = pageSize,
                Result = items
            };
        }

        public async static Task<QueryResponse<S>> Create<S>(IQueryable<S> source, int? queryPage, int? queryPageSize)
        {
            return await Create(source, queryPage, queryPageSize, d => d);
        }

        private static int getPageCount(int count, int pageSize)
        {
            int q = count / pageSize;
            int r = count % pageSize;

            return q + (r != 0 ? 1 : 0);
        }
    }
}
