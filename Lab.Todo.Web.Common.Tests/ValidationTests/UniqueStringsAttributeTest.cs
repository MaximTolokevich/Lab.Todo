using Lab.Todo.Web.Common.Validation;
using System.Collections.Generic;
using Xunit;

namespace Lab.Todo.Web.Common.Tests.ValidationTests
{
    public class UniqueStringsAttributeTests
    {
        [Theory]
        [MemberData(nameof(GetStringCollectionCaseSensitive))]
        public void WhenIgnoreCaseIsFalse_ValidationTheory(IEnumerable<string> stringCollection, bool expectedResult)
        {
            // Arrange
            var uniqueStringsAttribute = new UniqueStringsAttribute();

            // Act
            var isValid = uniqueStringsAttribute.IsValid(stringCollection);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetStringCollectionCaseSensitive()
        {
            yield return new object[] { null, true };
            yield return new object[] { new List<string> { "tag", "TAG", "Tag" }, true };
            yield return new object[] { new List<string> { "tag1", "tag2" }, true };
            yield return new object[] { new List<string> { "tag", "tag", "tag" }, false };
        }

        [Theory]
        [MemberData(nameof(GetStringCollectionCaseInsensitive))]
        public void WhenIgnoreCaseIsTrue_ValidationTheory(IEnumerable<string> stringCollection, bool expectedResult)
        {
            // Arrange
            var uniqueStringsAttribute = new UniqueStringsAttribute(true);

            // Act
            var isValid = uniqueStringsAttribute.IsValid(stringCollection);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetStringCollectionCaseInsensitive()
        {
            yield return new object[] { null, true };
            yield return new object[] { new List<string> { "tag", "TAG", "Tag" }, false };
            yield return new object[] { new List<string> { "tag1", "tag2" }, true };
            yield return new object[] { new List<string> { "tag", "tag", "tag" }, false };
        }
    }
}
