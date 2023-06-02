using System;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions
{
    public class TagDuplicateException : Exception
    {
        public TagDuplicateException(string message) : base(message) { }
    }
}