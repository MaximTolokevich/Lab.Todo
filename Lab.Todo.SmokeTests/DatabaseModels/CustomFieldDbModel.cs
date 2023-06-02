using System;
using System.ComponentModel.DataAnnotations.Schema;
using Lab.Todo.DAL.Helpers;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("CustomField")]
    public class CustomFieldDbModel
    {
        public int CustomFieldId { get; set; }
        public int ToDoItemId { get; set; }
        public int Order { get; set; }
        public CustomFieldDbEntryTypes Type { get; set; }
        public string? Name { get; set; }
        public string? StringValue { get; set; }
        public int? IntValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
    }
}