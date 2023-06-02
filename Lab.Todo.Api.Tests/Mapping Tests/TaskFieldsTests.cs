using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.Api.Tests.Helpers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Xunit;

namespace Lab.Todo.Api.Tests.MappingTests
{
    public class TaskFieldsTests
    {
        private readonly IMapper _mapper;

        public TaskFieldsTests()
        {
            _mapper = ApiTestsHelper.SetupMapper();
        }

        [Fact]
        public void Should_Return_Successfully_Mapped_ToDoItemCreateData_From_ToDoItemCreateRequest()
        {
            // Arrange
            var toDoItemCreateRequest = new ToDoItemCreateRequest
            {
                Title = "Title",
                Description = "Description",
                ParentTaskId = 14,
                Duration = TimeSpan.FromHours(3),
                Deadline = new DateTime(2020, 10, 10),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var mappedToDoItemCreateData = _mapper.Map<ToDoItemCreateData>(toDoItemCreateRequest);

            // Assert
            var expectedToDoItemCreateData = new ToDoItemCreateData
            {
                Title = "Title",
                Description = "Description",
                ParentTaskId = 14,
                Duration = TimeSpan.FromHours(3),
                PlannedStartTime = new DateTime(2000, 1, 1),
                Tags = new List<string>(),
                Deadline = new DateTime(2020, 10, 10),
                CustomFields = new List<CustomFieldBase>(),
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData>()
            };

            mappedToDoItemCreateData
                .Should().BeEquivalentTo(expectedToDoItemCreateData);
        }

        [Fact]
        public void Should_Return_Successfully_Mapped_ToDoItemGetResponse_From_ToDoItem()
        {
            // Arrange
            var toDoItem = new ToDoItem
            {
                Title = "Title",
                Description = "Description",
                ParentTask = new ToDoItem
                {
                    Id = 3,
                    Title = "title123",
                    Description = "descr",
                },
                Duration = TimeSpan.FromHours(3),
                ActualStartTime = DateTime.UtcNow,
                ActualEndTime = DateTime.UtcNow,
                Status = ToDoItemStatus.Completed,
                Deadline = null,
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var mappedToDoItemGetResponse = _mapper.Map<ToDoItemGetResponse>(toDoItem);

            // Assert
            var expectedToDoItemGetResponse = new ToDoItemGetResponse
            {
                Title = "Title",
                Description = "Description",
                ParentTask = new ParentTaskGetResponse
                {
                    Id = 3,
                    Title = "title123"
                },
                Duration = TimeSpan.FromHours(3),
                ActualStartTime = DateTime.UtcNow,
                ActualEndTime = DateTime.UtcNow,
                Status = ToDoItemStatus.Completed,
                Deadline = null,
                PlannedStartTime = new DateTime(2000, 1, 1),
                Tags = new List<string>(),
                CustomFields = new List<CustomFieldGetResponse>(),
                DependsOnItems = new List<ToDoItemDependencyGetResponse>()
            };

            mappedToDoItemGetResponse
                .Should().BeEquivalentTo(expectedToDoItemGetResponse, opt => opt.Excluding(item => item.ActualStartTime).Excluding(item => item.ActualEndTime));

            mappedToDoItemGetResponse.ActualStartTime.Should().BeCloseTo((DateTime)expectedToDoItemGetResponse.ActualStartTime, TimeSpan.FromSeconds(1));
            mappedToDoItemGetResponse.ActualEndTime.Should().BeCloseTo((DateTime)expectedToDoItemGetResponse.ActualEndTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_Return_Successfully_Mapped_ToDoItemUpdateData_From_ToDoItemUpdateRequest()
        {
            // Arrange
            var toDoItemUpdateRequest = new ToDoItemUpdateRequest
            {
                Title = "Title",
                Description = "Description",
                ParentTaskId = 14,
                Duration = TimeSpan.FromHours(3),
                Status = ToDoItemStatus.InProgress,
                Deadline = new DateTime(2020, 12, 11),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var mappedToDoItemUpdateData = _mapper.Map<ToDoItemUpdateData>(toDoItemUpdateRequest);

            // Assert
            var expectedToDoItemUpdateData = new ToDoItemUpdateData
            {
                Title = "Title",
                Description = "Description",
                ParentTaskId = 14,
                Duration = TimeSpan.FromHours(3),
                Status = ToDoItemStatus.InProgress,
                Deadline = new DateTime(2020, 12, 11),
                PlannedStartTime = new DateTime(2000, 1, 1),
                Tags = new List<string>(),
                CustomFields = new List<CustomFieldBase>(),
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData>()
            };

            mappedToDoItemUpdateData
                .Should().BeEquivalentTo(expectedToDoItemUpdateData);
        }

        [Fact]
        public void Should_Return_Successfully_Mapped_ReturnToDoItemCreateResponse_From_ToDoItem()
        {
            // Arrange
            var toDoItem = new ToDoItem
            {
                ActualStartTime = DateTime.UtcNow,
                Status = ToDoItemStatus.Planned,
                ActualEndTime = null,
                Duration = null,
                ParentTask = null,
                Id = 5,
                Deadline = new DateTime(2020, 12, 11),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var toDoItemCreateResponse = _mapper.Map<ToDoItemCreateResponse>(toDoItem);

            // Assert
            var expectedToDoItemCreateResponse = new ToDoItemCreateResponse
            {
                Status = ToDoItemStatus.Planned,
                Duration = null,
                Id = 5,
                Deadline = new DateTime(2020, 12, 11),
                PlannedStartTime = new DateTime(2000, 1, 1),
                Tags = new List<string>(),
                CustomFields = new List<CustomFieldGetResponse>(),
                DependsOnItems = new List<int>()
            };
            toDoItemCreateResponse.Should().BeEquivalentTo(expectedToDoItemCreateResponse);
        }
    }
}