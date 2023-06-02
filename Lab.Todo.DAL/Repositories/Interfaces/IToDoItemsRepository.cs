using Lab.Todo.DAL.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface IToDoItemsRepository
    {
        // ReSharper disable once UnusedMember.Global
        Task<IEnumerable<ToDoItemDbEntry>> GetAllAsync(bool trackEntities = false);

        Task<ToDoItemDbEntry> GetByIdAsync(int id, bool trackEntity = false);
        Task<IEnumerable<ToDoItemDbEntry>> GetByIdAsync(IEnumerable<int> ids, bool trackEntities = false);
        void Create(ToDoItemDbEntry toDoItem);
        void Update(ToDoItemDbEntry toDoItem);
        void Delete(int id);

        // ReSharper disable once UnusedMember.Global
        void Delete(ToDoItemDbEntry toDoItem);

        Task<IEnumerable<int>> GetAllIdsAsync();
        Task<Dictionary<int, string>> GetTitlesByIdsAsync(IEnumerable<int> ids);
        Task<bool> HasChildTasksAsync(int id);
        IQueryable<ToDoItemDbEntry> GetAllAsQueryable(bool trackEntities = false);
    }
}