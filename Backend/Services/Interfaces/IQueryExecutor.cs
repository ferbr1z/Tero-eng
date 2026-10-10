namespace MbyteWeb.Server.Services.Interfaces
{
    public interface IQueryExecutor
    {
        public Task Execute(Action action, int tries = 5, int delayMs = 5);
        public Task Execute(Func<Task> function, int tries = 5, int delayMs = 5);
        public Task<R> Execute<R>(Func<R> function, int tries = 5, int delayMs = 5);
        public Task<R> Execute<R>(Func<Task<R>> function, int tries = 5, int delayMs = 5);
    }
}
