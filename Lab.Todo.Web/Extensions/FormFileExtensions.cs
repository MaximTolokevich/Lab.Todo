using Microsoft.AspNetCore.Http;
using System.IO;

namespace Lab.Todo.Web.Extensions
{
    public static class FormFileExtensions
    {
        public static byte[] ToByteArray(this IFormFile file)
        {
            using var memoryStream = new MemoryStream();

            file.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
