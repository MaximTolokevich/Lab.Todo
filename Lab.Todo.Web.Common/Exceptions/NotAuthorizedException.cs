using System;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.Web.Common.Exceptions
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() { }
        public NotAuthorizedException(string message) : base(message) { }
        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}