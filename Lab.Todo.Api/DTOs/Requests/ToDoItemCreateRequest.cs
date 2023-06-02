﻿using Lab.Todo.Web.Common.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace Lab.Todo.Api.DTOs.Requests
{
    public class ToDoItemCreateRequest
    {
        [Required, MaxLength(250)]
        public string Title { get; set; }
        public string Description { get; set; }
        [MaxLength(10), StringLengthList(1, 50), UniqueStrings(true)]
        public IEnumerable<string> Tags { get; set; }
        public TimeSpan? Duration { get; set; }
        public int? ParentTaskId { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        [MaxLength(10)]
        public IEnumerable<int> DependsOnItems { get; set; }
        public IEnumerable<CustomFieldCreateRequest> CustomFields { get; set; }
    }
}