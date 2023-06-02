using System.Threading.Tasks;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Refit;

namespace Lab.Todo.SmokeTests.Services.DataAccess
{
    public interface IAccountService
    {
        [Post("/api/Account/Authenticate")]
        Task<IApiResponse<AuthenticateResponse?>> Authenticate([Body] AuthenticateRequest authenticateRequest);
    }
}