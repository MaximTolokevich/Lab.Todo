using Lab.Todo.BLL.Services.UserServices.Models;

namespace Lab.Todo.BLL.Services.UserServices
{
    public interface IUserService
    {
        User Current { get; }
    }
}
