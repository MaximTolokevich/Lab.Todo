using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Api.DTOs.Requests
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }
    }
}