using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}