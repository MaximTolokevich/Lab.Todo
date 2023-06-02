using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Lab.Todo.DAL.Attachments.Services;
using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Attachments.TrackedAttachment;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lab.Todo.DAL.Tests.AttachmentsTests
{
    public class FileTransactionServiceTests
    {
        private readonly Mock<IFileStorageService> _fileStorageMock = new();
        private readonly IFileTransactionService _fileTransactionService;

        public FileTransactionServiceTests()
        {
            var mockLogger = Mock.Of<ILogger<FileTransactionService>>();
            _fileTransactionService = new FileTransactionService(_fileStorageMock.Object, mockLogger);
        }

        [Fact]
        public async Task SaveChangesAsync_MarksCommitted_WhenSucceeds()
        {
            // Arrange
            var trackedFiles = new List<TrackedAttachment>
            {
                new TrackedAttachment { UniqueFileName = "1.png", Value = new byte[]{ 1, 2, 3 }, AttachmentState = AttachmentState.Added },
                new TrackedAttachment { UniqueFileName = "2.png", Value = new byte[]{ 4, 5, 6 }, AttachmentState = AttachmentState.Added },
                new TrackedAttachment { UniqueFileName = "3.png", Value = new byte[]{ 7, 8, 9 }, AttachmentState = AttachmentState.Added }
            };

            // Act
            await _fileTransactionService.SaveChangesAsync(trackedFiles);

            // Assert
            var expectedResult = new List<TrackedAttachment>
            {
                new TrackedAttachment { UniqueFileName = "1.png", Value = new byte[]{ 1, 2, 3 }, AttachmentState = AttachmentState.Added, IsCommitted = true },
                new TrackedAttachment { UniqueFileName = "2.png", Value = new byte[]{ 4, 5, 6 }, AttachmentState = AttachmentState.Added, IsCommitted = true },
                new TrackedAttachment { UniqueFileName = "3.png", Value = new byte[]{ 7, 8, 9 }, AttachmentState = AttachmentState.Added, IsCommitted = true }
            };

            trackedFiles.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task UndoChangesAsync_MarksUncommitted_WhenSucceeds()
        {
            // Arrange
            var trackedFiles = new List<TrackedAttachment>
            {
                new TrackedAttachment { UniqueFileName = "1.png", Value = new byte[]{ 1, 2, 3 }, AttachmentState = AttachmentState.Added, IsCommitted = true },
                new TrackedAttachment { UniqueFileName = "2.png", Value = new byte[]{ 4, 5, 6 }, AttachmentState = AttachmentState.Added, IsCommitted = true },
                new TrackedAttachment { UniqueFileName = "3.png", Value = new byte[]{ 7, 8, 9 }, AttachmentState = AttachmentState.Added }
            };

            // Act
            await _fileTransactionService.UndoChangesAsync(trackedFiles);

            // Assert
            var expectedResult = new List<TrackedAttachment>
            {
                new TrackedAttachment{ UniqueFileName = "1.png", Value = new byte[]{ 1, 2, 3 }, AttachmentState = AttachmentState.Added },
                new TrackedAttachment{ UniqueFileName = "2.png", Value = new byte[]{ 4, 5, 6 }, AttachmentState = AttachmentState.Added },
                new TrackedAttachment{ UniqueFileName = "3.png", Value = new byte[]{ 7, 8, 9 }, AttachmentState = AttachmentState.Added }
            };

            trackedFiles.Should().BeEquivalentTo(expectedResult);
        }
    }
}