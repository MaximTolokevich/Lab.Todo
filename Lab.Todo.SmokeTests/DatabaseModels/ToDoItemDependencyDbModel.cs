using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("ToDoItemDependency")]
    public class ToDoItemDependencyDbModel
    {
        public int ToDoItemId { get; set; }
        public int DependsOnToDoItemId { get; set; }
    }
}