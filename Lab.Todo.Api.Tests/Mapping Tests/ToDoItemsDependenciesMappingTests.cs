using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.Api.Tests.Helpers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Xunit;

namespace Lab.Todo.Api.Tests.MappingTests
{
    public class ToDoItemsDependenciesMappingTests
    {
        private readonly IMapper _mapper;

        public ToDoItemsDependenciesMappingTests()
        {
            _mapper = ApiTestsHelper.SetupMapper();
        }

        [Fact]
        public void Should_CorrectlyMapCollectionOfInt_ToCollectionOf_ToDoItemDependencyCreateUpdateData()
        {
            // Arrange
            var dependencies = new List<int> { 1, 5, 10 };

            var expectedResult = new List<ToDoItemDependencyCreateUpdateData> {
                new ToDoItemDependencyCreateUpdateData{ToDoItemId = 1},
                new ToDoItemDependencyCreateUpdateData{ToDoItemId = 5},
                new ToDoItemDependencyCreateUpdateData{ToDoItemId = 10},
            };

            // Act
            var mappingResults = _mapper.Map<IEnumerable<ToDoItemDependencyCreateUpdateData>>(dependencies);

            // Assert
            mappingResults.Should().BeEquivalentTo(expectedResult);
        }
        [Fact]
        public void Should_CorrectlyMapCollectionOfToDoItemDependency_ToCollectionOf_ToDoItemDependencyGetResponse()
        {
            // Arrange
            var dependencies = new List<ToDoItemDependency> {
                new ToDoItemDependency{ToDoItemId = 1,ToDoItemTitle="first"},
                new ToDoItemDependency{ToDoItemId = 3,ToDoItemTitle="second"},
                new ToDoItemDependency{ToDoItemId = 5,ToDoItemTitle="third"}
            };

            var expectedResult = new List<ToDoItemDependencyGetResponse> {
                new ToDoItemDependencyGetResponse{ToDoItemId = 1,ToDoItemTitle="first"},
                new ToDoItemDependencyGetResponse{ToDoItemId = 3,ToDoItemTitle="second"},
                new ToDoItemDependencyGetResponse{ToDoItemId = 5,ToDoItemTitle="third"},
            };

            // Act
            var mappingResults = _mapper.Map<List<ToDoItemDependencyGetResponse>>(dependencies);

            // Assert
            mappingResults.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void Should_CorrectlyMapCollectionOfToDoItemDependency_ToCollectionOf_Int()
        {
            // Arrange
            var dependencies = new List<ToDoItemDependency> {
                new ToDoItemDependency{ToDoItemId = 1,ToDoItemTitle="first"},
                new ToDoItemDependency{ToDoItemId = 3,ToDoItemTitle="second"},
                new ToDoItemDependency{ToDoItemId = 5,ToDoItemTitle="third"}
            };

            var expectedResult = new List<int> { 1, 3, 5 };

            // Act
            var mappingResults = _mapper.Map<IEnumerable<int>>(dependencies);

            // Assert
            mappingResults.Should().BeEquivalentTo(expectedResult);
        }
    }
}
