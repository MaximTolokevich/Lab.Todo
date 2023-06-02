using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Lab.Todo.BLL.Services.AttachmentManagers.Models;
using Lab.Todo.BLL.Services.UniqueFileNameServices;
using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;

namespace Lab.Todo.BLL.Services.AttachmentManagers
{
    public class AttachmentManager : IAttachmentManager
    {
        private readonly IMapper _mapper;
        private readonly IUniqueFileNameService _fileNameService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public AttachmentManager(IUnitOfWork unitOfWork, IUniqueFileNameService fileNameService, IFileStorageService fileStorageService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileNameService = fileNameService;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<Attachment> UploadAttachmentAsync(AttachmentCreateData attachment) 
        {
            if (attachment.Content is null || attachment.Content.Length == 0)
            {
                throw new ArgumentException($"Property {nameof(attachment.Content)} can't be null or empty");
            }

            var attachmentDbEntry = _mapper.Map<AttachmentDbEntry>(attachment);
            var extensionWithoutDot = Path.GetExtension(attachment.ProvidedFileName)?[1..];
            attachmentDbEntry.UniqueFileName = _fileNameService.GetUniqueFileName(extensionWithoutDot);

            _unitOfWork.Attachments.Create(attachmentDbEntry);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<Attachment>(attachmentDbEntry);
        }

        public async Task DeleteAttachmentAsync(string name)
        {
            var uniqueName = await _unitOfWork.Attachments.GetUniqueNameAsync(name);
            await _unitOfWork.Attachments.DeleteAsync(uniqueName);
            await _fileStorageService.DeleteAsync(uniqueName);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Attachment> GetAttachmentAsync(string name)
        {
            var attachmentDbEntry = await _unitOfWork.Attachments.GetByNameAsync(name);
            var attachment = _mapper.Map<Attachment>(attachmentDbEntry);
            attachment.Content = await _fileStorageService.GetAsync(name);

            return attachment;
        }
    }
}