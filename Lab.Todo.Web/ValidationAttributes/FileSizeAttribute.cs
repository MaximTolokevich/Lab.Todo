using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.Web.ValidationAttributes
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class FileSizeAttribute : RangeAttribute
    {
        public FileSizeAttribute(int minimumSize, int maximumSize) : base(minimumSize, maximumSize) { }

        public override bool IsValid(object value) => base.IsValid(((IFormFile)value)?.Length);
    }
}
