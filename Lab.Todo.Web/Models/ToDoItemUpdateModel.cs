using Lab.Todo.Web.Common.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;

namespace Lab.Todo.Web.Models
{
    public class ToDoItemUpdateModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(250, ErrorMessage = "{0} max length: {1}")]
        public string Title { get; set; }

        [Display(Name = nameof(Description))]
        public string Description { get; set; }
        public TagViewModel TagViewModel { get; set; } = new();
        public StatusViewModel StatusModel { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string AssignedTo { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public int? ParentTaskId { get; set; }
        public DateTime? Deadline { get; set; }

        [MaxLength(10, ErrorMessage = "{0} maximum length: {1}")]
        [StringLengthList(1, 50)]
        [UniqueStrings(true)]
        public IEnumerable<string> Tags { get; set; }

        public ToDoItemAddDependencyViewModel DependencyViewModel { get; set; }

        [Display(Name = "Custom Fields")]
        public IEnumerable<CustomFieldUpdateModel> CustomFields { get; set; }

        public ToDoItemStatus Status { get; set; }
    }
}