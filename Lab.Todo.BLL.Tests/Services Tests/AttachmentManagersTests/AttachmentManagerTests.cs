using System;
using System.Threading.Tasks;
using FluentAssertions;
using Lab.Todo.BLL.Services.AttachmentManagers;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;
using Lab.Todo.BLL.Services.UniqueFileNameServices;
using Lab.Todo.BLL.Tests.Helpers;
using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Moq;
using Xunit;

namespace Lab.Todo.BLL.Tests.ServicesTests.AttachmentManagersTests
{
    public class AttachmentManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockAttachmentsUnitOfWork = new();
        private readonly Mock<IAttachmentsRepository> _mockAttachments = new();
        private readonly Mock<IUniqueFileNameService> _mockFileNameService = new();
        private readonly Mock<IFileStorageService> _mockFileStorageService = new();
        private readonly IAttachmentManager _attachmentManager;

        public AttachmentManagerTests()
        {
            _mockAttachmentsUnitOfWork.Setup(uow => uow.Attachments).Returns(_mockAttachments.Object);

            var mapper = BLLTestsHelper.SetupMapper();
            _attachmentManager = new AttachmentManager(_mockAttachmentsUnitOfWork.Object,
                _mockFileNameService.Object,
                _mockFileStorageService.Object,
                mapper);
        }

        [Fact]
        public async Task UploadAttachmentAsync_ReturnsNewAttachment_WhenSucceeds()
        {
            // Arrange
            var attachmentCreateInfo = new AttachmentCreateData { ProvidedFileName = "text.txt", Content = new byte[] { 1, 2, 3 } };

            const string uniqueFileName = "unique.txt";
            _mockFileNameService.Setup(service => service.GetUniqueFileName(It.IsAny<string>()))
                .Returns(uniqueFileName);

            // Act
            var returnedAttachment = await _attachmentManager.UploadAttachmentAsync(attachmentCreateInfo);

            // Assert
            var expectedAttachment = new Attachment
            {
                Id = 0,
                ProvidedFileName = attachmentCreateInfo.ProvidedFileName,
                UniqueFileName = uniqueFileName,
                Content = attachmentCreateInfo.Content
            };

            returnedAttachment.Should().BeEquivalentTo(expectedAttachment);
        }

        [Fact]
        public async Task UploadAttachmentAsync_ThrowsException_WhenContentIsNull()
        {
            // Arrange
            var attachmentCreateInfo = new AttachmentCreateData { ProvidedFileName = "image.png" };

            // Act
            Func<Task> act = async () => await _attachmentManager.UploadAttachmentAsync(attachmentCreateInfo);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Property Content can't be null or empty");
        }

        [Fact]
        public async Task UploadAttachmentAsync_ThrowsException_WhenContentIsEmpty()
        {
            // Arrange
            var attachmentCreateInfo = new AttachmentCreateData { ProvidedFileName = "image.png", Content = Array.Empty<byte>() };

            // Act
            Func<Task> act = async () => await _attachmentManager.UploadAttachmentAsync(attachmentCreateInfo);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Property Content can't be null or empty");
        }

        [Fact]
        public async Task UploadAttachmentAsync_ReturnsDifferentAttachments_WhenFileNamesAreSame()
        {
            // Arrange
            var firstAttachmentCreateInfo = new AttachmentCreateData { ProvidedFileName = "text.txt", Content = new byte[] { 1, 2, 3 } };
            var secondAttachmentCreateInfo = new AttachmentCreateData { ProvidedFileName = "text.txt", Content = new byte[] { 4, 5, 6 } };

            _mockFileNameService.SetupSequence(service => service.GetUniqueFileName(It.IsAny<string>()))
                .Returns("unique1.txt")
                .Returns("unique2.txt");

            // Act
            var firstReturnedAttachment = await _attachmentManager.UploadAttachmentAsync(firstAttachmentCreateInfo);
            var secondReturnedAttachment = await _attachmentManager.UploadAttachmentAsync(secondAttachmentCreateInfo);

            // Assert
            firstReturnedAttachment.UniqueFileName.Should().NotBeEquivalentTo(secondReturnedAttachment.UniqueFileName);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_Should_GetUniqueName_And_BasedOnThis_DeleteAttachment()
        {
            // Arrange
            _mockAttachments
                .Setup(rep => rep.GetUniqueNameAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<string>());

            _mockAttachments.Setup(rep => rep.DeleteAsync(It.IsAny<string>()));

            _mockFileStorageService.Setup(uow => uow.DeleteAsync(It.IsAny<string>()));

            // Act
            await _attachmentManager.DeleteAttachmentAsync(It.IsAny<string>());

            // Assert
            _mockAttachmentsUnitOfWork.Verify(v => v.SaveChangesAsync());

            _mockAttachments.VerifyAll();
            _mockFileStorageService.VerifyAll();
        }

        [Fact]
        public async Task GetAttachmentAsync_Should_Return_Successfully_Mapped_Attachment()
        {
            // Arrange
            const string fileName = "file.txt";

            var attachmentDbEntry = new AttachmentDbEntry
            {
                Id = 0,
                ProvidedFileName = "ProvidedName",
                UniqueFileName = "UniqueName"
            };

            _mockAttachments.Setup(rep => rep.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(attachmentDbEntry);
            _mockFileStorageService.Setup(uow => uow.GetAsync(It.IsAny<string>())).ReturnsAsync(new byte[] { 1, 2, 3 });

            // Act
            var returnedAttachment = await _attachmentManager.GetAttachmentAsync(fileName);

            // Assert
            var expectedAttachment = new Attachment
            {
                Id = 0,
                ProvidedFileName = "ProvidedName",
                UniqueFileName = "UniqueName",
                Content = new byte[] { 1, 2, 3 }
            };

            returnedAttachment
                .Should().BeEquivalentTo(expectedAttachment);
        }
    }
}
