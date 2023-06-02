using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("ToDoItemTagAssociation")]
    public class ToDoItemTagAssociationDbModel
    {
        public int ToDoItemId { get; set; }
        public int TagId { get; set; }
        public int Order { get; set; }
    }
}