﻿using Lab.Todo.Web.Common.MappingHelpers;

namespace Lab.Todo.Web.Models
{
    public class CustomFieldCreateModel
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public CustomFieldTypes Type { get; set; }
        public object Value { get; set; }
    }
}
