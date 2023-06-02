using System;

namespace Lab.Todo.Api.DTOs.Responses
{
    public class DevExceptionResponse : ExceptionResponse
    {
        public Type Type { get; set; }
        public string StackTrace { get; set; }
        public Exception InnerException { get; set; }
    }
}