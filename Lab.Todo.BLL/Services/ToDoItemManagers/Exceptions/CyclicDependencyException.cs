using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class CyclicDependencyException : Exception
    {
        public IEnumerable<(int SourceId, IEnumerable<int> IdsCycle)> CyclicDependencies { get; set; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public CyclicDependencyException() { }

        public CyclicDependencyException(string message) : base(message) { }
    }
}