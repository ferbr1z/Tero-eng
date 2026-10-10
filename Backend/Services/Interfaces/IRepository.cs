using MbyteWeb.Server.Models;
using System;

namespace MbyteWeb.Server.Services.Interfaces
{
    public interface IRepository<T> where T : class, IModel, ISoftDeletable
    {
        public Task<T?> GetOrDefault(int id);
        public Task<T> Get(int id);
        public IQueryable<T> GetAll();
        public void Add(T entity);
        public void Update(T entity);
        public void Delete(T entity);
    }
}
