using System.Diagnostics.CodeAnalysis;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;
using System.Threading.Tasks;

namespace Lab.Todo.BLL.Services.AttachmentManagers
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IAttachmentManager
    {
        Task DeleteAttachmentAsync(string name);
        Task<Attachment> GetAttachmentAsync(string name);
        Task<Attachment> UploadAttachmentAsync(AttachmentCreateData attachmentCreateData);
    }
}