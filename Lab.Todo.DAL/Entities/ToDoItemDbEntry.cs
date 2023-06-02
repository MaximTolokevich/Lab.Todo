using Lab.Todo.DAL.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.DAL.Entities
{
    [Table("ToDoItem")]
    public class ToDoItemDbEntry : IEntity, ICreationAuditFields, IModificationAuditFields
    {
        [Column("ToDoItemId")]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ToDoItemTagAssociationDbEntry> ToDoItemTagAssociations { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? PlannedStartTime { get; set; }

        public int? ParentTaskId { get; set; } 
        public virtual ToDoItemDbEntry ParentTask { get; set; }

        [Required]
        public int ToDoItemStatusId { get; set; } = 1;

        [InverseProperty(nameof(ToDoItemDependencyDbEntry.ToDoItem))]
        public ICollection<ToDoItemDependencyDbEntry> DependsOnItems { get; set; }

        [InverseProperty(nameof(ToDoItemDependencyDbEntry.DependsOnToDoItem))]
        public ICollection<ToDoItemDependencyDbEntry> DependentItems { get; set; }
        public ICollection<CustomFieldDbEntry> CustomFields { get; set; }
    }
}