using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class UnauthorizedUserException : Exception
    {
        public UnauthorizedUserException(string message) : base(message) { }
    }
}