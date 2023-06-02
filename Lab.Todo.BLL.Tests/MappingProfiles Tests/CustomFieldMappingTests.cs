using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.BLL.Tests.Helpers;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Helpers;
using Xunit;

namespace Lab.Todo.BLL.Tests.MappingProfilesTests
{
    public class CustomFieldMappingTests
    {
        private readonly IMapper _mapper;
        private static DateTime _date;

        public CustomFieldMappingTests()
        {
            _mapper = BLLTestsHelper.SetupMapper();
            _date = DateTime.UtcNow;
        }

        [Fact]
        public void Should_MapCollectionOf_ICustomField_ToCollectionOf_CustomFieldDbEntry()
        {
            // Arrange         
            IEnumerable<CustomFieldBase> modelList = GetICustomFieldCollection();

            // Act
            var mappedList = _mapper.Map<IEnumerable<CustomFieldDbEntry>>(modelList);

            // Assert
            mappedList.Should().BeEquivalentTo(GetCustomFieldDbEntryCollection());
        }

        [Fact]
        public void Should_MapCollectionOf_CustomFieldDbEntry_ToCollectionOf_ICustomField()
        {
            // Arrange
            var dbEntryList = GetCustomFieldDbEntryCollection();

            // Act
            var mappedList = _mapper.Map<IEnumerable<CustomFieldBase>>(dbEntryList);

            // Assert
            mappedList.Should().BeEquivalentTo(GetICustomFieldCollection());
        }

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

        private static IEnumerable<CustomFieldBase> GetICustomFieldCollection()
        {
            var model1 = new CustomField<int>
            {
                Id = 1,
                Order = 1,
                Name = "Field1",
                Value = 125
            };
            var model2 = new CustomField<DateTime>
            {
                Id = 2,
                Order = 2,
                Name = "Field2",
                Value = _date
            };
            var model3 = new CustomField<string>
            {
                Id = 3,
                Order = 3,
                Name = "Field3",
                Value = "stringvalue"
            };

            var list = new List<CustomFieldBase>
            {
                model1,
                model2,
                model3
            };

            return list;
        }

        private static IEnumerable<CustomFieldDbEntry> GetCustomFieldDbEntryCollection()
        {
            var model1 = new CustomFieldDbEntry
            {
                Id = 1,
                Order = 1,
                Name = "Field1",
                Type = CustomFieldDbEntryTypes.IntType,
                IntValue = 125
            };
            var model2 = new CustomFieldDbEntry
            {
                Id = 2,
                Order = 2,
                Name = "Field2",
                Type = CustomFieldDbEntryTypes.DateTimeType,
                DateTimeValue = _date
            };
            var model3 = new CustomFieldDbEntry
            {
                Id = 3,
                Order = 3,
                Name = "Field3",
                Type = CustomFieldDbEntryTypes.StringType,
                StringValue = "stringvalue"
            };

            var list = new List<CustomFieldDbEntry>
            {
                model1,
                model2,
                model3
            };

            return list;
        }
    }
}
