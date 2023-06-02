using Lab.Todo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Lab.Todo.Web.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionFilter> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ExceptionFilter(ILogger<ExceptionFilter> logger, IHostEnvironment hostEnvironment, IModelMetadataProvider modelMetadataProvider)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, $"{context.Exception.Message}{Environment.NewLine}Trace identifier: {context.HttpContext.TraceIdentifier}");

            context.Result = new ViewResult
            {
                ViewName = "../Error/Error",
                StatusCode = (int)HttpStatusCode.BadRequest,
                ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                {
                    Model = new ErrorViewModel(context.Exception, context.HttpContext, _hostEnvironment.IsDevelopment())
                }
            };

            context.ExceptionHandled = true;
        }
    }
}