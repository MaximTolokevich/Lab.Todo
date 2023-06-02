using Lab.Todo.Web.Common.Validation;
using Lab.Todo.Web.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.Web.Models
{
    public class AttachmentCreateModel
    {
        [Required(ErrorMessage = "Please select a file.")]
        [FileNameLength(200, ErrorMessage = "File name can't be longer than 200 characters.")]
        [FileSize(1, 5 * 1024 * 1024, ErrorMessage = "File can't be empty or larger than 5 MB.")]
        [FileExtensionLength(1, 10)]
        public IFormFile File { get; set; }
    }
}
