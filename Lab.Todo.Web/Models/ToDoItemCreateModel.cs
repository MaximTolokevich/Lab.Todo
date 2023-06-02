using Lab.Todo.Web.Common.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Web.Models
{
    public class ToDoItemCreateModel
    {
        [Required]
        [MaxLength(250, ErrorMessage = "{0} max length: {1}")]
        public string Title { get; set; }

        public string Description { get; set; }

        public TagViewModel TagViewModel { get; set; } = new();
        
        [MaxLength(10)]
        [StringLengthList(1, 50)]
        [UniqueStrings(true)]
        public IEnumerable<string> Tags { get; set; }

        public TimeSpan? Duration { get; set; }
        public int? ParentTaskId { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public IEnumerable<CustomFieldCreateModel> CustomFields { get; set; }
        public ToDoItemAddDependencyViewModel DependencyViewModel { get; set; } = new();
    }
}