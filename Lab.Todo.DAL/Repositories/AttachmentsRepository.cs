using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Todo.DAL.Attachments.TrackedAttachment;

namespace Lab.Todo.DAL.Repositories
{
    public class AttachmentsRepository : EfRepository<AttachmentDbEntry>, IAttachmentsRepository
    {
        private readonly ICollection<TrackedAttachment> _trackedAttachments;

        public AttachmentsRepository(DbContext context, ICollection<TrackedAttachment> trackedAttachments) : base(context)
        {
            _trackedAttachments = trackedAttachments;
        }

        public override void Create(AttachmentDbEntry attachment)
        {
            base.Create(attachment);

            _trackedAttachments.Add(new TrackedAttachment
            {
                UniqueFileName = attachment.UniqueFileName,
                Value = attachment.Content,
                AttachmentState = AttachmentState.Added
            });
        }

        public async Task<AttachmentDbEntry> GetByNameAsync(string name)
        {
            return await Table.FirstOrDefaultAsync(file => file.UniqueFileName == name);
        }

        public async Task DeleteAsync(string name)
        {
            var deletedItem = await Table.FirstOrDefaultAsync(file => file.UniqueFileName == name);

            Table.Remove(deletedItem);
        }

        public async Task<string> GetUniqueNameAsync(string providedName)
        {
            var item = await Table.FirstOrDefaultAsync(item => item.ProvidedFileName == providedName);

            return item.UniqueFileName;
        }
    }
}