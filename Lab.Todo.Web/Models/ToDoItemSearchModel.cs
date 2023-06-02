using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using System;
using System.Collections.Generic;

namespace Lab.Todo.Web.Models
{
    public class ToDoItemSearchModel
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
        public IEnumerable<CustomFieldSearchModel> CustomFields { get; set; }
        public IEnumerable<IEnumerable<string>> Tags { get; set; }
        public IEnumerable<ToDoItemStatus> AllStatuses { get; set; }
    }
}