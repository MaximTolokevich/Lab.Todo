using Lab.Todo.DAL.Entities;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface IAttachmentsRepository
    {
        void Create(AttachmentDbEntry attachment);
        Task<AttachmentDbEntry> GetByNameAsync(string name);
        Task DeleteAsync(string name);
        Task<string> GetUniqueNameAsync(string providedName);
    }
}