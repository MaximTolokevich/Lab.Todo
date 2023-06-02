using Lab.Todo.Web.Common.MappingHelpers;

namespace Lab.Todo.Api.DTOs.Requests
{
    public class CustomFieldUpdateRequest
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public CustomFieldTypes Type { get; set; }
        public object Value { get; set; }
    }
}