using Lab.Todo.Web.Common.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Web.Models
{
    public class TagViewModel
    {
        [MaxLength(10), StringLengthList(1, 50), UniqueStrings(true)]
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<TagModel> PredefinedTagSuggestions { get; set; }
        public IEnumerable<TagModel> NotPredefinedTagSuggestions { get; set; }
    }
}
