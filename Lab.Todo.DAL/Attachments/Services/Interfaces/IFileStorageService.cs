using System.Threading.Tasks;

namespace Lab.Todo.DAL.Attachments.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<byte[]> GetAsync(string fileName);
        Task UploadAsync(string fileName, byte[] fileContents);
        Task DeleteAsync(string fileName);
    }
}