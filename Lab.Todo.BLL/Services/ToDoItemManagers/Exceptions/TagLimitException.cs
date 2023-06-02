using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class TagLimitException : Exception
    {
        public TagLimitException(string message) : base(message) { }
    }
}