using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Attachments.Services.Options;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Attachments.Services
{
    public class AzureStorageService : IFileStorageService
    {
        private readonly AzureStorageOptions _options;
        private BlobContainerClient _blobContainerClient;

        public AzureStorageService(IOptions<AzureStorageOptions> options)
        {
            _options = options.Value;
            var blobServiceClient = new BlobServiceClient(_options.ConnectionStringPrimary);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        }

        public async Task<byte[]> GetAsync(string fileName)
        {
            CheckConnection();
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            await using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);

            return stream.ToArray();
        }

        public async Task UploadAsync(string fileName, byte[] fileContents)
        {
            CheckConnection();
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            await using var stream = new MemoryStream(fileContents);

            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = MediaTypeNames.Application.Octet });
        }

        public async Task DeleteAsync(string fileName)
        {
            CheckConnection();
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        private void CheckConnection()
        {
            try
            {
                _blobContainerClient.CreateIfNotExists();
            }
            catch
            {
                SwitchConnection();

                _blobContainerClient.CreateIfNotExists();
            }
        }

        private void SwitchConnection()
        {
            var blobServiceClient = new BlobServiceClient(_options.ConnectionStringSecondary);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        }
    }
}