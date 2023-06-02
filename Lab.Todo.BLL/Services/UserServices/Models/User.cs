using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.BLL.Services.UserServices.Models
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}