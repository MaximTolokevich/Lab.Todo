using Lab.Todo.DAL.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class EfRepository<T> where T : class, IEntity, new()
    {
        protected readonly DbContext Context;
        protected readonly DbSet<T> Table;

        protected EfRepository(DbContext context)
        {
            Context = context;
            Table = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool trackEntities = false)
        {
            var query = (trackEntities == false) ? Table.AsNoTracking() : Table;

            return await query.ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id, bool trackEntity = false)
        {
            var query = (trackEntity == false) ? Table.AsNoTracking() : Table;

            return await query.FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public virtual void Create(T entity) => Table.Add(entity);

        public virtual void Update(T entity) => Table.Update(entity);

        public virtual void Delete(int id)
        {
            var entity = Context.ChangeTracker.Entries<T>().FirstOrDefault(entry => entry.Entity.Id == id)?.Entity;
            entity ??= new T { Id = id };

            Table.Remove(entity);
        }

        public virtual void Delete(T entity) => Table.Remove(entity);
    }
}