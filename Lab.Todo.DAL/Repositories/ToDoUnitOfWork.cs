using Lab.Todo.DAL.Attachments.Services.Interfaces;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Lab.Todo.DAL.Attachments.TrackedAttachment;

namespace Lab.Todo.DAL.Repositories
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class ToDoUnitOfWork : DbContext, IUnitOfWork
    {
        private IToDoItemsRepository _itemsRepository;
        private IAttachmentsRepository _attachmentsRepository;
        private ITagsRepository _tagsRepository;
        private ICustomFieldsRepository _customFieldsRepository;
        private IToDoItemTagAssociationsRepository _toDoItemTagAssociationsRepository;
        private IToDoItemStatusesRepository _statusesRepository;
        private IToDoItemDependenciesRepository _dependenciesRepository;

        private readonly ICollection<TrackedAttachment> _trackedAttachments = new List<TrackedAttachment>();
        private readonly IFileTransactionService _fileTransactionService;
        private readonly ILogger<ToDoUnitOfWork> _logger;

        public IToDoItemsRepository ToDoItems => _itemsRepository ??= new ToDoItemsRepository(this);
        public IToDoItemStatusesRepository ToDoItemStatuses => _statusesRepository ??= new ToDoItemStatusesRepository(this);
        public IAttachmentsRepository Attachments => _attachmentsRepository ??= new AttachmentsRepository(this, _trackedAttachments);
        public ITagsRepository Tags => _tagsRepository ??= new TagsRepository(this);
        public IToDoItemTagAssociationsRepository ToDoItemTagAssociations
            => _toDoItemTagAssociationsRepository ??= new ToDoItemTagAssociationsRepository(this);
        public IToDoItemDependenciesRepository ToDoItemDependencies
            => _dependenciesRepository ??= new ToDoItemDependenciesRepository(this);
        public ICustomFieldsRepository CustomFields => _customFieldsRepository ??= new CustomFieldsRepository(this);

        private DbSet<ToDoItemDbEntry> ToDoItemsTable { get; set; }
        private DbSet<ToDoItemStatusDbEntry> ToDoItemStatusesTable { get; set; }
        private DbSet<AttachmentDbEntry> AttachmentsTable { get; set; }
        private DbSet<CustomFieldDbEntry> CustomFieldsTable { get; set; }
        private DbSet<TagDbEntry> TagsTable { get; set; }
        private DbSet<ToDoItemTagAssociationDbEntry> ToDoItemTagAssociationsTable { get; set; }
        private DbSet<ToDoItemDependencyDbEntry> ToDoItemDependenciesTable { get; set; }
        private DbSet<CyclicDependencyNodeDbEntry> CyclicDependenciesNodes { get; set; }

        public ToDoUnitOfWork(DbContextOptions<ToDoUnitOfWork> options, IFileTransactionService fileTransactionService, ILogger<ToDoUnitOfWork> logger) : base(options)
        {
            _fileTransactionService = fileTransactionService;
            _logger = logger;
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _fileTransactionService.SaveChangesAsync(_trackedAttachments);
                await base.SaveChangesAsync();

                _logger.LogInformation("The changes were successfully saved to the database.");
            }
            catch
            {
                await _fileTransactionService.UndoChangesAsync(_trackedAttachments);

                _logger.LogError("Saving changes to the database passed with an error. Undo of changes.");

                throw;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoItemTagAssociationDbEntry>()
                .HasKey(toDoItemAssociation => new { toDoItemAssociation.ToDoItemId, toDoItemAssociation.TagId });

            modelBuilder.Entity<ToDoItemDependencyDbEntry>()
                .HasKey(dep => new { dep.ToDoItemId, dep.DependsOnToDoItemId });

            modelBuilder.Entity<ToDoItemDbEntry>()
                .HasOne(item => item.ParentTask)
                .WithOne()
                .HasForeignKey<ToDoItemDbEntry>(item => item.ParentTaskId)
                .HasPrincipalKey<ToDoItemDbEntry>(item => item.Id);
        }
    }
}