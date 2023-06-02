using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lab.Todo.DAL.Entities.Interfaces;
using Lab.Todo.DAL.Helpers;

namespace Lab.Todo.DAL.Entities
{
    [Table("CustomField")]
    public class CustomFieldDbEntry : IEntity
    {
        [Column("CustomFieldId")]
        public int Id { get; set; }

        public int ToDoItemId { get; set; }

        public ToDoItemDbEntry ToDoItem { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public CustomFieldDbEntryTypes Type { get; set; }

        [Required]
        public string Name { get; set; }

        public string StringValue { get; set; }

        public int? IntValue { get; set; }

        public DateTime? DateTimeValue { get; set; }
    }
}