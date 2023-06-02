using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories
{
    public class ToDoItemsRepository : EfRepository<ToDoItemDbEntry>, IToDoItemsRepository
    {
        public ToDoItemsRepository(DbContext context) : base(context) { }
        
        public override async Task<IEnumerable<ToDoItemDbEntry>> GetAllAsync(bool trackEntities = false)
        {
            var query = GetAllAsQueryable(trackEntities);
            return await query
                .ToListAsync();
        }

        public override async Task<ToDoItemDbEntry> GetByIdAsync(int id, bool trackEntity = false)
        {
            var query = GetAllAsQueryable(trackEntity);
            return await query
                .FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public async Task<IEnumerable<ToDoItemDbEntry>> GetByIdAsync(IEnumerable<int> ids, bool trackEntities = false)
        {
            var query = GetAllAsQueryable(trackEntities);
            return await query
                .Where(toDoItem => ids.Contains(toDoItem.Id))
                .ToListAsync();
        }

        public override void Update(ToDoItemDbEntry toDoItem)
        {
            base.Update(toDoItem);
            Context.Entry(toDoItem).Property(item => item.CreatedBy).IsModified = false;
        }

        public async Task<bool> HasChildTasksAsync(int id) => await Table.AnyAsync(item => item.ParentTaskId == id);

        public async Task<IEnumerable<int>> GetAllIdsAsync() => await Table.Select(item => item.Id).ToListAsync();

        public async Task<Dictionary<int, string>> GetTitlesByIdsAsync(IEnumerable<int> ids)
        {
            return await Table
                .Where(toDoItem => ids.Contains(toDoItem.Id))
                .Select(item => new { item.Id, item.Title })
                .ToDictionaryAsync(item => item.Id, item => item.Title);
        }

        public IQueryable<ToDoItemDbEntry> GetAllAsQueryable(bool trackEntities = false)
        {
            var query = (trackEntities == false) ? Table.AsNoTracking() : Table;
            return query
                .Include(item => item.DependsOnItems)
                .Include(item => item.DependentItems)
                .Include(item => item.ParentTask)
                .Include(item => item.CustomFields)
                .Include(item => item.ToDoItemTagAssociations)
                .ThenInclude(toDoItemTagAssociation => toDoItemTagAssociation.Tag);
        }
    }
}