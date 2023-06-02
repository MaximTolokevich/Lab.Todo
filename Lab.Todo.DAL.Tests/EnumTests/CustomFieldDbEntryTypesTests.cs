using Lab.Todo.DAL.Helpers;
using System.Collections.Generic;
using Xunit;

namespace Lab.Todo.DAL.Tests.EnumTests
{
    public class CustomFieldDbEntryTypesTests
    {
        [Theory]
        [MemberData(nameof(GetCustomFieldDbEntryTypes))]
        public void CustomFieldDbEntryTypes_Should_Have_Right_Value(CustomFieldDbEntryTypes type, int expectedTypeNumber)
        {
            // Arrange
            int typeNumber = (int)type;

            // Assert
            Assert.Equal(typeNumber, expectedTypeNumber);
        }

        public static IEnumerable<object[]> GetCustomFieldDbEntryTypes()
        {
            yield return new object[] { CustomFieldDbEntryTypes.IntType, 1 };
            yield return new object[] { CustomFieldDbEntryTypes.StringType, 2 };
            yield return new object[] { CustomFieldDbEntryTypes.DateTimeType, 3 };
        }
    }
}
