using Lab.Todo.BLL.Services.TokenHandler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Lab.Todo.Api.Extensions
{
    public static class JwtSecurityTokenHandlerExtensions
    {
        public static IServiceCollection AddJwtSecurityTokenHandler(this IServiceCollection services) =>
            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>()
                    .AddTransient<ISecurityTokenHandler, SecurityTokenHandlerAdapter>();
    }
}
