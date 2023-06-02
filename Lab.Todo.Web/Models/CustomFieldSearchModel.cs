using Lab.Todo.Web.Common.MappingHelpers;

namespace Lab.Todo.Web.Models
{
    public class CustomFieldSearchModel
    {
        public string Name { get; set; }
        public CustomFieldTypes Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
    }
}