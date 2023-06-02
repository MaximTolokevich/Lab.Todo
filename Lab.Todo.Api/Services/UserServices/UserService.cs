using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.BLL.Services.UserServices.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace Lab.Todo.Api.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = httpContextAccessor;
        }

        public User Current => BuildUserModel(_contextAccessor.HttpContext?.User.Claims);

        private static User BuildUserModel(IEnumerable<Claim> claims)
        {
            if (claims is null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var userLogin = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(userLogin))
            {
                throw new BadHttpRequestException("Token's claims don't contain an user data.",
                    (int)HttpStatusCode.Unauthorized);
            }

            return new User
            {
                Name = userLogin,
                Email = userLogin
            };
        }
    }
}