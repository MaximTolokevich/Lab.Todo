using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class ExtraTagException : Exception
    {
        public ExtraTagException(string message) : base(message) { }
    }
}