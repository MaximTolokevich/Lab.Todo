using Lab.Todo.Web.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lab.Todo.Api.DTOs.Requests
{
    public class AttachmentCreateRequest
    {
        [Required, MaxLength(200), FileExtensionLength(1, 10)]
        public string FileName { get; set; }

        [Required, MinLength(1), MaxLength(5 * 1024 * 1024)]
        [JsonPropertyName("contentAsBase64")]
        public byte[] Content { get; set; }
    }
}
