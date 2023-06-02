using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("Tag")]
    public class TagDbModel 
    {
        public int TagId { get; set; }
        public string? Value { get; set; }
        public bool IsPredefined { get; set; }
    }
}