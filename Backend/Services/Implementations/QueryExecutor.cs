using MbyteWeb.Server.Data;
using MbyteWeb.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MbyteWeb.Server.Services.Implementations
{
    public class QueryExecutor(AppDbContext context) : IQueryExecutor
    {
        #region Execute Functions
        public async Task Execute(Action action, int tries = 5, int delayMs = 5)
        {
            await Try(action, tries, delayMs);
        }

        public async Task Execute(Func<Task> function, int tries = 5, int delayMs = 5)
        {
            await Try(function, tries, delayMs);
        }

        public async Task<R> Execute<R>(Func<R> action, int tries = 5, int delayMs = 5)
        {
            return await Try(action, tries, delayMs);
        }

        public async Task<R> Execute<R>(Func<Task<R>> function, int tries = 5, int delayMs = 5)
        {
            return await Try(function, tries, delayMs);
        }

        private async Task Try(Action action, int tries = 5, int delayMs = 100)
        {
            do
            {
                try
                {
                    action();
                    await context.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    tries--;
                    await Task.Delay(delayMs);
                    context.ChangeTracker.Clear();
                    if (tries <= 0) throw;
                }
            } while (tries > 0);

            //Unreachable
            throw new Exception("Unexpected repository execution error.");
        }

        private async Task Try(Func<Task> function, int tries = 5, int delayMs = 100)
        {
            do
            {
                try
                {
                    await function();
                    await context.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    tries--;
                    await Task.Delay(delayMs);
                    context.ChangeTracker.Clear();
                    if (tries <= 0) throw;
                }
            } while (tries > 0);

            //Unreachable
            throw new Exception("Unexpected repository execution error.");
        }

        private async Task<R> Try<R>(Func<R> function, int tries = 5, int delayMs = 100)
        {
            do
            {
                try
                {
                    var result = function();
                    await context.SaveChangesAsync();
                    return result;
                }
                catch (DbUpdateConcurrencyException)
                {
                    tries--;
                    await Task.Delay(delayMs);
                    context.ChangeTracker.Clear();
                    if (tries <= 0) throw;
                }
            } while (tries > 0);

            //Unreachable
            throw new Exception("Unexpected repository execution error.");
        }

        private async Task<R> Try<R>(Func<Task<R>> function, int tries = 5, int delayMs = 100)
        {
            do
            {
                try
                {
                    var result = await function();
                    await context.SaveChangesAsync();
                    return result;
                }
                catch (DbUpdateConcurrencyException)
                {
                    tries--;
                    await Task.Delay(delayMs);
                    context.ChangeTracker.Clear();
                    if (tries <= 0) throw;
                }
            } while (tries > 0);

            //Unreachable
            throw new Exception("Unexpected repository execution error.");
        }
        #endregion
    }
}
