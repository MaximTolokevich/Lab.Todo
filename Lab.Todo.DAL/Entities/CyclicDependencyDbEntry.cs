using System.Collections.Generic;

namespace Lab.Todo.DAL.Entities
{
    public class CyclicDependencyDbEntry
    {
        public IEnumerable<int> IdsCycle { get; set; }
    }
}