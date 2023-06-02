using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Lab.Todo.Api.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionFilter(IWebHostEnvironment environment, ILogger<ExceptionFilter> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var exceptionType = exception.GetType();

            var statusCode = GetStatusCode(exception);

            var response = GetExceptionResponse();

            if (response is DevExceptionResponse devExceptionResponse)
            {
                devExceptionResponse.Message = exception.Message;
                devExceptionResponse.Type = exceptionType;
                devExceptionResponse.StackTrace = exception.StackTrace;
                devExceptionResponse.InnerException = exception.InnerException;
            }
            else
            {
                var isUnhandledException = statusCode == StatusCodes.Status500InternalServerError;
                var message = isUnhandledException ? "Something went wrong on the server..." : exception.Message;
                response.Message = message;
            }

            response.StatusCode = statusCode;

            _logger.LogError(exception, exception.Message);

            context.Result = new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                UnauthorizedUserException or
                CyclicDependencyException or
                WrongDependencyException or
                TagLimitException or
                ExtraTagException => StatusCodes.Status400BadRequest,
                PermissionsException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private ExceptionResponse GetExceptionResponse()
        {
            return _environment.IsDevelopment() ? new DevExceptionResponse() : new ExceptionResponse();
        }
    }
}