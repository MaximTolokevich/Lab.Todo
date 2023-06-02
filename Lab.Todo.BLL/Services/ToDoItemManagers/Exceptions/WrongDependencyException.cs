using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class WrongDependencyException : Exception
    {
        public WrongDependencyException(string message) : base(message) { }
    }
}