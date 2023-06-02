using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class PermissionsException : Exception
    {
        public PermissionsException(string message) : base(message) { }
    }
}