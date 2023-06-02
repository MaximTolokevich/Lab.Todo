using System;
using System.Collections.Generic;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Models
{
    public class SearchOptions
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ParentTaskId { get; set; }
        public IEnumerable<ToDoItemStatus> Statuses { get; set; }
        public TimeSpan? DurationFrom { get; set; }
        public TimeSpan? DurationTo { get; set; }
        public DateTime? ActualStartTimeFrom { get; set; }
        public DateTime? ActualStartTimeTo { get; set; }
        public DateTime? ActualEndTimeFrom { get; set; }
        public DateTime? ActualEndTimeTo { get; set; }
        public DateTime? DeadlineFrom { get; set; }
        public DateTime? DeadlineTo { get; set; }
        public DateTime? PlannedStartTimeFrom { get; set; }
        public DateTime? PlannedStartTimeTo { get; set; }
        public IEnumerable<CustomFieldsSearchOptionsBase> CustomFields { get; set; }
        public IEnumerable<IEnumerable<string>> Tags { get; set; }
    }
}