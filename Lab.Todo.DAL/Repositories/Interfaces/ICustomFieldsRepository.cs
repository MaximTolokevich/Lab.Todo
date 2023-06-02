namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface ICustomFieldsRepository
    {
        void DeleteRelatedToToDoItem(int toDoItemId);
    }
}
