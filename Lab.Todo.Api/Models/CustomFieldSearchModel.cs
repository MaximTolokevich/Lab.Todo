using Lab.Todo.Web.Common.MappingHelpers;

namespace Lab.Todo.Api.Models
{
    public class CustomFieldSearchModel
    {
        public CustomFieldTypes Type { get; set; }
        public string Name { get; set; }
        public Range<object> Value { get; set; }
    }
}
