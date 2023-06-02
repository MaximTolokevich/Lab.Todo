namespace Lab.Todo.BLL.Services.ToDoItemManagers.Models
{
    public class CustomField<T> : CustomFieldBase
    {
        public T Value { get; set; }
    }
}