using Lab.Todo.Web.Common.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Web.Models
{
    public class TagsRemovalModel
    {
        [Required, MinLength(1)]
        public IEnumerable<int> ToDoItemIds { get; set; }

        [Required, MinLength(1), MaxLength(10), StringLengthList(1, 50), UniqueStrings(true)]
        public IEnumerable<string> Tags { get; set; }
    }
}
