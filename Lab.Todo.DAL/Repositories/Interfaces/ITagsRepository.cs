using Lab.Todo.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface ITagsRepository
    {
        Task<IEnumerable<TagDbEntry>> GetPredefinedAsync(bool trackEntities = false);
        Task<IEnumerable<TagDbEntry>> GetByValuesAsync(IEnumerable<string> values, bool trackEntity = false);
        void Create(TagDbEntry tag);
    }
}
