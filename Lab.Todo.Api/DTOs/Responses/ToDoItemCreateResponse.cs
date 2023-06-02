using System;
using System.Collections.Generic;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;

namespace Lab.Todo.Api.DTOs.Responses
{
    public class ToDoItemCreateResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<int> DependsOnItems { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? Deadline { get; set; }
        public ToDoItemStatus Status { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public ParentTaskGetResponse ParentTask { get; set; }
        public IEnumerable<CustomFieldGetResponse> CustomFields { get; set; }
    }
}