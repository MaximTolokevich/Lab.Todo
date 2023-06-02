using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lab.Todo.Web.Common.Validation
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueStringsAttribute : ValidationAttribute
    {
        private readonly bool _ignoreCase;

        public UniqueStringsAttribute(bool ignoreCase = false)
        {
            _ignoreCase = ignoreCase;
            ErrorMessage = "{0} have one or more duplicates.";
        }

        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            var comparer = (_ignoreCase is false) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            return ((IEnumerable<string>)value).GroupBy(s => s, comparer).All(group => group.Count() == 1);
        }
    }
}