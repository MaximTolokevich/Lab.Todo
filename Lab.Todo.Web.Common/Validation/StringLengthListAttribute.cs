using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.Web.Common.Validation
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [AttributeUsage(AttributeTargets.Property)]
    public class StringLengthListAttribute : ValidationAttribute
    {
        private readonly int _minimumLength;
        private readonly int _maximumLength;

        public StringLengthListAttribute(int minimumLength, int maximumLength)
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
            ErrorMessage = $"{{0}} can contain strings with a minimum length of {_minimumLength} " +
                $"and a maximum length of {_maximumLength}.";
        }

        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            foreach (var str in (IEnumerable<string>)value)
            {
                if (str is null || str.Length > _maximumLength || str.Length < _minimumLength)
                {
                    return false;
                }
            }

            return true;
        }
    }
}