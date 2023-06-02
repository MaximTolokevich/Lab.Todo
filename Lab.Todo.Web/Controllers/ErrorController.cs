using Microsoft.AspNetCore.Mvc;

namespace Lab.Todo.Web.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error()
        {
            return View();
        }
    }
}