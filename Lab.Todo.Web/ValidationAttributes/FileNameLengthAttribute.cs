using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.Web.ValidationAttributes
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [AttributeUsage(AttributeTargets.Property)]
    public class FileNameLengthAttribute : StringLengthAttribute
    {
        public FileNameLengthAttribute(int maximumLength) : base(maximumLength) { }

        public override bool IsValid(object value) => base.IsValid(((IFormFile)value)?.FileName);
    }
}
