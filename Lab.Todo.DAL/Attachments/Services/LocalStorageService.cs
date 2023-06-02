using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Attachments.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Attachments.Services
{
    public class LocalStorageService : IFileStorageService
    {
        private readonly LocalStorageOptions _options;
        private readonly ILogger<LocalStorageService> _logger;

        public LocalStorageService(IOptions<LocalStorageOptions> options, ILogger<LocalStorageService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<byte[]> GetAsync(string fileName)
        {
            var absolutePath = BuildPathToFile(fileName);
            await using var fileStream = File.Open(absolutePath, FileMode.Open, FileAccess.Read, FileShare.None);

            var fileContents = new byte[fileStream.Length];

            await fileStream.ReadAsync(fileContents);
            _logger.LogInformation($"File \"{fileName}\" got successfully.");

            return fileContents;
        }

        public async Task UploadAsync(string fileName, byte[] fileContents)
        {
            var absolutePath = BuildPathToFile(fileName);
            await using var fileStream = File.Open(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

            await fileStream.WriteAsync(fileContents);
            _logger.LogInformation($"File \"{fileName}\" created successfully.");
        }

        public Task DeleteAsync(string fileName)
        {
            var absolutePath = BuildPathToFile(fileName);
            File.Delete(absolutePath);
            _logger.LogInformation($"File \"{fileName}\" deleted successfully.");

            return Task.CompletedTask;
        }

        private string BuildPathToFile(string relativePath)
        {
            string folderPath = Path.GetFullPath(_options.FolderPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, relativePath);
        }
    }
}