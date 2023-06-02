using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IToDoItemsRepository ToDoItems { get; }
        IToDoItemStatusesRepository ToDoItemStatuses { get; }
        IAttachmentsRepository Attachments { get; }
        ITagsRepository Tags { get; }
        IToDoItemTagAssociationsRepository ToDoItemTagAssociations { get; }
        IToDoItemDependenciesRepository ToDoItemDependencies { get; }
        ICustomFieldsRepository CustomFields { get; }

        Task SaveChangesAsync();
    }
}