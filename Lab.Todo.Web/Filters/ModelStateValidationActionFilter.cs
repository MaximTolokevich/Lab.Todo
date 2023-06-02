using Lab.Todo.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Lab.Todo.Web.Filters
{
    public class ModelStateValidationActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var ex = new NotValidModelStateException("ModelState is not valid.");
                var errorMessages = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                for (var index = 0; index < errorMessages.Length; index++)
                {
                    ex.Data["ex" + (index + 1)] = errorMessages.ElementAt(index);
                }

                throw ex;
            }

            base.OnActionExecuting(context);
        }
    }
}
