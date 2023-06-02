using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.BLL.Services.UserServices.Models;
using Lab.Todo.Web.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Lab.Todo.Web.Common.Services
{
    public class UserService : IUserService
    {
        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            Current = BuildUserModel(httpContextAccessor.HttpContext);
        }

        public User Current { get; }

        private static User BuildUserModel(HttpContext httpContext)
        {
            if (httpContext.User.Identity is not null)
            {
                return new User { Email = httpContext.User.Identity.Name };
            }

            throw new NotAuthorizedException("User is not authorized");
        }
    }
}