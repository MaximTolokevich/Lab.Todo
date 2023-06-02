using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.DAL.Entities
{
    [Table("ToDoItemDependency")]
    public class ToDoItemDependencyDbEntry
    {
        public int ToDoItemId { get; set; }
        public int DependsOnToDoItemId { get; set; }

        [ForeignKey("ToDoItemId")]
        public ToDoItemDbEntry ToDoItem { get; set; }

        [ForeignKey("DependsOnToDoItemId")]
        public ToDoItemDbEntry DependsOnToDoItem { get; set; }
    }
}