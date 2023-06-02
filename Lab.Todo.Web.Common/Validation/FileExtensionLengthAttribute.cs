using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Lab.Todo.Web.Common.Validation
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class FileExtensionLengthAttribute : ValidationAttribute
    {
        private readonly int _minimumLength;
        private readonly int _maximumLength;

        public FileExtensionLengthAttribute(int minimumLength, int maximumLength)
        {
            if (minimumLength <= 0)
            {
                throw new ArgumentException("Length arguments must be greater than 0.");
            }

            if (minimumLength > maximumLength)
            {
                throw new ArgumentException("Minimum length can't be greater than maximum length.");
            }

            _minimumLength = minimumLength;
            _maximumLength = maximumLength;
            ErrorMessage = $"File extension can be {minimumLength}-{maximumLength} characters long.";
        }

        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            var fileName = value switch
            {
                IFormFile formFile => formFile.FileName,
                string => value.ToString(),
                _ => throw new ArgumentException("This attribute supports only IFormFile and string properties."),
            };

            if (fileName is null)
            {
                throw new ApplicationException("File name is null");
            }

            var extensionWithDot = Path.GetExtension(fileName);

            var extension = extensionWithDot.Length == 0 ? extensionWithDot : extensionWithDot[1..];

            return extension.Length >= _minimumLength && extension.Length <= _maximumLength;
        }
    }
}