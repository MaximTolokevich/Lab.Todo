using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Attachments.Services.Interfaces
{
    public interface IFileTransactionService
    {
        Task SaveChangesAsync(ICollection<TrackedAttachment.TrackedAttachment> trackedFiles);
        Task UndoChangesAsync(ICollection<TrackedAttachment.TrackedAttachment> trackedFiles);
    }
}
