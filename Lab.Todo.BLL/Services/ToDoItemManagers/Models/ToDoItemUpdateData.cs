using System;
using System.Collections.Generic;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Models
{
    public class ToDoItemUpdateData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<ToDoItemDependencyCreateUpdateData> DependsOnItems { get; set; }
        public ToDoItemStatus Status { get; set; }
        public IEnumerable<CustomFieldBase> CustomFields { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public int? ParentTaskId { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
