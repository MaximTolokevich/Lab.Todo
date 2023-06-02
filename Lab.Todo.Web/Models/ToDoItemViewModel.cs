using System;
using System.Collections.Generic;

namespace Lab.Todo.Web.Models
{
    public class ToDoItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> DependsOnItems { get; set; }
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
        public ParentTaskViewModel ParentTask { get; set; }
        public string Status { get; set; }
        public IEnumerable<CustomFieldViewModel> CustomFields { get; set; }
    }
}