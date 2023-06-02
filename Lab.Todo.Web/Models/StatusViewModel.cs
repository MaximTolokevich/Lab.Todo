using System.Collections.Generic;

namespace Lab.Todo.Web.Models
{
    public class StatusViewModel
    {
        public string CurrentStatus { get; set; }
        public IEnumerable<string> StatusSuggestions { get; set; }
    }
}