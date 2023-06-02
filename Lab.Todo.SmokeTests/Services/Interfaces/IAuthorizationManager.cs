using System.Threading.Tasks;

namespace Lab.Todo.SmokeTests.Services.Interfaces
{
    public interface IAuthorizationManager
    {
        Task<string?> GetAuthorizationToken();
        Task<IUserContext> AuthorizeAsUser(string username);
    }
}