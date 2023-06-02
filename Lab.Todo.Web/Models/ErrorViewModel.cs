using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Lab.Todo.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestPath { get; set; }
        public string TraceIdentifier { get; set; }
        public IEnumerable<ExceptionViewModel> ExceptionModels { get; set; }

        public ErrorViewModel(Exception ex, HttpContext httpContext, bool isDevelopmentEnvironment)
        {
            RequestPath = httpContext.Request.Path;
            TraceIdentifier = httpContext.TraceIdentifier;

            List<ExceptionViewModel> exceptions = new();

            while (ex is not null)
            {
                var exceptionViewModel = new ExceptionViewModel(ex);
                if (!isDevelopmentEnvironment)
                {
                    exceptionViewModel.StackTrace = null;
                }

                exceptions.Add(exceptionViewModel);
                ex = ex.InnerException;
            }

            ExceptionModels = exceptions;
        }
    }
}