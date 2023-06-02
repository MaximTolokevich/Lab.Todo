using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("ToDoItemStatus")]
    public class ToDoItemStatusDbModel
    {
        public int ToDoItemStatusId { get; set; }

        public string? Name { get; set; }
    }
}