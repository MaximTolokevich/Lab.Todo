using Lab.Todo.DAL.Entities.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.DAL.Entities
{
    [Table("Tag")]
    public class TagDbEntry : IEntity
    {
        [Column("TagId")]
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsPredefined { get; set; }
    }
}