using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("ToDoItem")]
    public class ToDoItemDbModel 
    {
        public int ToDoItemId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AssignedTo { get; set; }
        public string? CreatedBy { get; set; }
    }
}