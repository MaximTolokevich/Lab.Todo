using System;
using System.Collections.Generic;
using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.Api.Tests.Helpers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.Web.Common.MappingHelpers;
using Xunit;

namespace Lab.Todo.Api.Tests.MappingTests
{
    public class CustomFieldMappingTests
    {
        private readonly IMapper _mapper;
        private static DateTime _date;

        public CustomFieldMappingTests()
        {
            _mapper = ApiTestsHelper.SetupMapper();
            _date = DateTime.UtcNow;
        }

        [Fact]
        public void Should_CorrectlyMapCollectionOfCustomFieldCreateRequest_ToCollectionOf_ICustomField()
        {
            // Arrange         
            IEnumerable<CustomFieldCreateRequest> modelList = GetCustomFieldCreateRequestCollection();

            // Act
            var mappedList = _mapper.Map<IEnumerable<CustomFieldCreateRequest>, IEnumerable<CustomFieldBase>>(modelList);

            // Assert
            mappedList.Should().BeEquivalentTo(GetICustomFieldCollectionNewEntries());
        }

        [Fact]
        public void Should_CorrectlyMapCollectionOfCustomFieldUpdateRequest_ToCollectionOf_ICustomField()
        {
            // Arrange         
            IEnumerable<CustomFieldUpdateRequest> modelList = GetCustomFieldUpdateRequestCollection();

            // Act
            var mappedList = _mapper.Map<IEnumerable<CustomFieldUpdateRequest>, IEnumerable<CustomFieldBase>>(modelList);

            // Assert
            mappedList.Should().BeEquivalentTo(GetICustomFieldCollectionUpdatedEntries());
        }

        [Fact]
        public void Should_CorrectlyMapCollectionOf_ICustomField_ToCollectionOf_CustomFieldGetResponse()
        {
            // Arrange
            IEnumerable<CustomFieldBase> customFields = GetICustomFieldCollectionNewEntries();

            // Act
            var mappedList = _mapper.Map<IEnumerable<CustomFieldBase>, IEnumerable<CustomFieldGetResponse>>(customFields);

            // Assert
            mappedList.Should().BeEquivalentTo(GetCustomFieldGetResponseCollection());
        }

        private static IEnumerable<CustomFieldCreateRequest> GetCustomFieldCreateRequestCollection()
        {
            var model1 = new CustomFieldCreateRequest
            {
                Name = "Field1",
                Order = 1,
                Type = CustomFieldTypes.Number,
                Value = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(123)).RootElement
            };
            var model2 = new CustomFieldCreateRequest
            {
                Name = "Field2",
                Order = 2,
                Type = CustomFieldTypes.DateTime,
                Value = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(_date)).RootElement
            };
            var model3 = new CustomFieldCreateRequest
            {
                Name = "Field3",
                Order = 3,
                Type = CustomFieldTypes.Text,
                Value = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes("stringvalue")).RootElement
            };

            var list = new List<CustomFieldCreateRequest>
            {
                model1,
                model2,
                model3
            };

            return list;
        }

        private static IEnumerable<CustomFieldBase> GetICustomFieldCollectionNewEntries()
        {
            var model1 = new CustomField<int>
            {
                Name = "Field1",
                Order = 1,
                Value = 123
            };
            var model2 = new CustomField<DateTime>
            {
                Name = "Field2",
                Order = 2,
                Value = _date
            };
            var model3 = new CustomField<string>
            {
                Name = "Field3",
                Order = 3,
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

        private static IEnumerable<CustomFieldGetResponse> GetCustomFieldGetResponseCollection()
        {
            var model1 = new CustomFieldGetResponse
            {
                Name = "Field1",
                Order = 1,
                Type = CustomFieldTypes.Number,
                Value = 123
            };
            var model2 = new CustomFieldGetResponse
            {
                Name = "Field2",
                Order = 2,
                Type = CustomFieldTypes.DateTime,
                Value = _date
            };
            var model3 = new CustomFieldGetResponse
            {
                Name = "Field3",
                Order = 3,
                Type = CustomFieldTypes.Text,
                Value = "stringvalue"
            };

            var list = new List<CustomFieldGetResponse>
            {
                model1,
                model2,
                model3
            };

            return list;
        }

        private static IEnumerable<CustomFieldUpdateRequest> GetCustomFieldUpdateRequestCollection()
        {
            var model1 = new CustomFieldUpdateRequest
            {
                Id = 1,
                Name = "Field1",
                Order = 1,
                Type = CustomFieldTypes.Number,
                Value = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(123)).RootElement
            };
            var model2 = new CustomFieldUpdateRequest
            {
                Id = 2,
                Name = "Field2",
                Order = 2,
                Type = CustomFieldTypes.DateTime,
                Value = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(_date)).RootElement
            };
            var model3 = new CustomFieldUpdateRequest
            {
                Name = "Field3",
                Order = 3,
                Type = CustomFieldTypes.Text,
                Value = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes("stringvalue")).RootElement
            };

            var list = new List<CustomFieldUpdateRequest>
            {
                model1,
                model2,
                model3
            };

            return list;
        }

        private static IEnumerable<CustomFieldBase> GetICustomFieldCollectionUpdatedEntries()
        {
            var model1 = new CustomField<int>
            {
                Id = 1,
                Name = "Field1",
                Order = 1,
                Value = 123
            };
            var model2 = new CustomField<DateTime>
            {
                Id = 2,
                Name = "Field2",
                Order = 2,
                Value = _date
            };
            var model3 = new CustomField<string>
            {
                Name = "Field3",
                Order = 3,
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
    }
}