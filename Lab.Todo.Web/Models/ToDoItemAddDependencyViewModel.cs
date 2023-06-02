using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Web.Models
{
    public class ToDoItemAddDependencyViewModel
    {
        public bool HasAvailableDependencies { get; set; }
        public IEnumerable<DependencyModel> AvailableDependencies { get; set; }
        public IEnumerable<(DependencyModel Source, IEnumerable<DependencyModel> Cycle)> CyclicDependencies { get; set; }

        [MaxLength(10)]
        public IEnumerable<int> DependsOnItems { get; set; }
    }
}