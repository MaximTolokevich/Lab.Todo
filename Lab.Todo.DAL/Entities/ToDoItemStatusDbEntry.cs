using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Lab.Todo.DAL.Entities.Interfaces;

namespace Lab.Todo.DAL.Entities
{
    [Table("ToDoItemStatus")]
    public class ToDoItemStatusDbEntry : IEntity
    {
        [Column("ToDoItemStatusId")]
        public int Id { get; set; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public string Name { get; set; }
    }
}