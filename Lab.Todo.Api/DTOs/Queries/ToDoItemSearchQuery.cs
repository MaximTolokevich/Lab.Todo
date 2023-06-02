using System;
using System.Collections.Generic;
using Lab.Todo.Api.Models;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;

namespace Lab.Todo.Api.DTOs.Queries
{
    public class ToDoItemSearchQuery
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ParentTaskId { get; set; }
        public IEnumerable<ToDoItemStatus> Statuses { get; set; }
        public Range<TimeSpan?> Duration { get; set; }
        public Range<DateTime?> ActualStartTime { get; set; }
        public Range<DateTime?> ActualEndTime { get; set; }
        public Range<DateTime?> Deadline { get; set; }
        public Range<DateTime?> PlannedStartTime { get; set; }
        public IEnumerable<CustomFieldSearchModel> CustomFields { get; set; }
        public IEnumerable<IEnumerable<string>> Tags { get; set; }
    }
}