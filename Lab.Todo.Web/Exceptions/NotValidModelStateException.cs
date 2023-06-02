using System;

namespace Lab.Todo.Web.Exceptions
{
    public class NotValidModelStateException : Exception
    {
        public NotValidModelStateException(string message) : base(message) { }
    }
}