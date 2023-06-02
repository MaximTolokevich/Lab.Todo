using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.DAL.Entities
{
    [Table("ToDoItemTagAssociation")]
    public class ToDoItemTagAssociationDbEntry
    {
        public int ToDoItemId { get; set; }
        public int TagId { get; set; }
        public int Order { get; set; }
        public ToDoItemDbEntry ToDoItem { get; set; }
        public TagDbEntry Tag { get; set; }
    }
}