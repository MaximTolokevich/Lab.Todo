using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Api.DTOs.Queries
{
    public class TagSuggestionsQuery
    {
        [Range(1, int.MaxValue)]
        public int? UsageTimeSpanInDays { get; set; }
    }
}
