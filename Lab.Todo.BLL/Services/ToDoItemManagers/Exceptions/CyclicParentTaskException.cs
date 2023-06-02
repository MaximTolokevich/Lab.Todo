using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CyclicParentTaskException : Exception
    {
        public IEnumerable<(int sourceId, IEnumerable<int> idsCycle)> CyclicParentTasks { get; set; }
        public CyclicParentTaskException(string message) : base(message) { }
    }
}