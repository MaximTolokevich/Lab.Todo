namespace Lab.Todo.BLL.Services.ToDoItemManagers.Models
{
    public class CustomFieldsSearchOptions<T> : CustomFieldsSearchOptionsBase
    {
        public T From { get; set; }
        public T To { get; set; }
    }
}