namespace Lab.Todo.BLL.Services.ToDoItemManagers.Models
{
    public abstract class CustomFieldBase
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
    }
}