using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Todo.DAL.Attachments.TrackedAttachment;

namespace Lab.Todo.DAL.Attachments.Services
{
    public class FileTransactionService : IFileTransactionService
    {
        private readonly IFileStorageService _fileService;
        private readonly ILogger<FileTransactionService> _logger;

        public FileTransactionService(IFileStorageService fileService, ILogger<FileTransactionService> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        public async Task SaveChangesAsync(ICollection<TrackedAttachment.TrackedAttachment> trackedFiles)
        {
            foreach (var trackedFile in trackedFiles)
            {
                await SaveSingleChangeAsync(trackedFile);
            }
            
            _logger.LogInformation("All changes have been successfully saved");
        }

        public async Task UndoChangesAsync(ICollection<TrackedAttachment.TrackedAttachment> trackedFiles)
        {
            foreach (var trackedFile in trackedFiles)
            {
                if (trackedFile.IsCommitted == false)
                {
                    _logger.LogInformation("An unconfirmed file was encountered, and changes to it were successfully rolled back.");
                    return;
                }

                await UndoSingleChangeAsync(trackedFile);
            }

            _logger.LogInformation("All changes were successfully undo");
        }

        private async Task SaveSingleChangeAsync(TrackedAttachment.TrackedAttachment trackedFile)
        {
            switch (trackedFile.AttachmentState)
            {
                case AttachmentState.Added:
                    await _fileService.UploadAsync(trackedFile.UniqueFileName, trackedFile.Value);
                    break;
            }

            trackedFile.IsCommitted = true;
        }

        private async Task UndoSingleChangeAsync(TrackedAttachment.TrackedAttachment trackedFile)
        {
            switch (trackedFile.AttachmentState)
            {
                case AttachmentState.Added:
                    await _fileService.DeleteAsync(trackedFile.UniqueFileName);
                    break;
            }

            trackedFile.IsCommitted = false;
        }
    }
}