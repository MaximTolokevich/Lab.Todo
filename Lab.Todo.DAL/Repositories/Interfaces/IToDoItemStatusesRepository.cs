using Lab.Todo.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface IToDoItemStatusesRepository
    {
        Task<IEnumerable<ToDoItemStatusDbEntry>> GetAllAsync(bool trackEntities = false);
    }
}
