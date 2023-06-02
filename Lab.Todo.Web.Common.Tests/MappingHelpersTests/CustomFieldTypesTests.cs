using System.Collections.Generic;
using Lab.Todo.Web.Common.MappingHelpers;
using Xunit;

namespace Lab.Todo.Web.Common.Tests.MappingHelpersTests
{
    public class CustomFieldTypesTests
    {
        [Theory]
        [MemberData(nameof(GetCustomFieldTypes))]
        public void CustomFieldDbEntryTypes_Should_Have_Right_Value(CustomFieldTypes type, int expectedTypeNumber)
        {
            //Arrange
            var typeNumber = (int)type;

            //Assert
            Assert.Equal(typeNumber, expectedTypeNumber);
        }

        public static IEnumerable<object[]> GetCustomFieldTypes()
        {
            yield return new object[] { CustomFieldTypes.Number, 1 };
            yield return new object[] { CustomFieldTypes.Text, 2 };
            yield return new object[] { CustomFieldTypes.DateTime, 3 };
        }
    }
}
