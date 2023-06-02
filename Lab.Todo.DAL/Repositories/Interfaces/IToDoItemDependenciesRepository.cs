using Lab.Todo.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface IToDoItemDependenciesRepository
    {
        void Delete(ToDoItemDependencyDbEntry entity);
        Task<IEnumerable<CyclicDependencyDbEntry>> GetCyclicDependenciesAsync(int toDoItemId, IEnumerable<int> dependenciesIds);
        Task<IEnumerable<CyclicDependencyDbEntry>> GetCyclicParentTaskAsync(int toDoItemId, int parentTaskId);
    }
}
