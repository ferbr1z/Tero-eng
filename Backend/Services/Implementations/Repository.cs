using MbyteWeb.Server.Data;
using MbyteWeb.Server.Exceptions;
using MbyteWeb.Server.Models;
using MbyteWeb.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MbyteWeb.Server.Services.Implementations
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : class, IModel, ISoftDeletable
    {
        protected AppDbContext context = context;

        public virtual async Task<T?> GetOrDefault(int id)
        {
            // FindAsync no admite filtro, por lo que se saltearía el soft delete; usamos
            // FirstOrDefaultAsync para excluir los registros borrados, igual que GetAll.
            return await context.Set<T>().FirstOrDefaultAsync(m => m.Id == id && !m.Borrado);
        }

        public virtual async Task<T> Get(int id)
        {
            var result = await GetOrDefault(id);
            if (result == null) throw new NotFoundException();
            return result;
        }

        public virtual IQueryable<T> GetAll()
        {
            return context.Set<T>().Where(m=>m.Borrado == false);
        }

        public virtual void Add(T entity)
        {
            context.Add(entity);
        }

        public virtual void Update(T entity)
        {
            context.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            if (entity == null) return;
            if (entity.Borrado) return;

            if (entity.Protegido) throw new BadRequestException("No se puede borrar esta entidad.");

            //Set borrado to true
            entity.Borrado = true;
        }
    }
}
