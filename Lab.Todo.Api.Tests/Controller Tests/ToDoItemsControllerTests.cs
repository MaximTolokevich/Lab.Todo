using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Lab.Todo.Api.Controllers;
using Lab.Todo.Api.DTOs.Queries;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.Tests.Helpers;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Lab.Todo.Api.Tests.ControllerTests
{
    public class ToDoItemsControllerTests
    {
        private readonly Mock<IToDoItemManager> _mockToDoItemManager;
        private readonly ToDoItemsController _controller;

        public ToDoItemsControllerTests()
        {
            var mapper = ApiTestsHelper.SetupMapper();
            _mockToDoItemManager = new Mock<IToDoItemManager>();
            _controller = new(mapper, _mockToDoItemManager.Object);
        }

        [Theory]
        [MemberData(nameof(GetItemCreateRequests))]
        public void ToDoItemCreateRequest_ModelStateValidationTheory(ToDoItemCreateRequest item, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(item);

            // Act
            var isValid = Validator.TryValidateObject(item, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetItemCreateRequests()
        {
            yield return new object[] { new ToDoItemCreateRequest { Title = "", Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = null, Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description" }, true };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "" }, true };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = null }, true };
            yield return new object[] { new ToDoItemCreateRequest { Title = ApiTestsHelper.GenerateString(251), Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = ApiTestsHelper.GenerateString(251), Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "tag1", "" } }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", Tags = new List<string> { ApiTestsHelper.GenerateString(51) } }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "tag1", "tag2", "urgent" } }, true };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "tag1", "tag2", "tag1" } }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "Tag", "tag", "TAG" } }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11" } }, false };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } }, true };
            yield return new object[] { new ToDoItemCreateRequest { Title = "Title", Description = "Description", DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } }, false };
        }

        [Theory]
        [MemberData(nameof(GetItemUpdateRequests))]
        public void ToDoItemUpdateRequest_ModelStateValidationTheory(ToDoItemUpdateRequest item, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(item);

            // Act
            var isValid = Validator.TryValidateObject(item, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetItemUpdateRequests()
        {
            yield return new object[] { new ToDoItemUpdateRequest { Title = "", Description = "Description", Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = null, Description = "Description", Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "", Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateRequest { Title = ApiTestsHelper.GenerateString(251), Description = "Description", Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "tag1", "" }, Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Tags = new List<string> { ApiTestsHelper.GenerateString(51) }, Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "tag1", "tag2", "urgent" }, Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "tag1", "tag2", "tag1" }, Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "Tag", "tag", "TAG" } }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11" } }, false };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateRequest { Title = "Title", Description = "Description", DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, Status = ToDoItemStatus.Planned }, false };
        }

        [Theory]
        [MemberData(nameof(GetTagsAdditionRequests))]
        public void TagsAdditionRequest_ModelStateValidationTheory(TagsAdditionRequest tagsAdditionRequest, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(tagsAdditionRequest);

            // Act
            var isValid = Validator.TryValidateObject(tagsAdditionRequest, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetTagsAdditionRequests()
        {
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = null, Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = null }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int>(), Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string>() }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11" } }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "tag1", "" } }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { ApiTestsHelper.GenerateString(51) } }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag", "tag", "TAG" } }, false };
            yield return new object[] { new TagsAdditionRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2" } }, true };
        }

        [Theory]
        [MemberData(nameof(GetTagsRemovalRequests))]
        public void TagsRemovalRequest_ModelStateValidationTheory(TagsRemovalRequest tagsRemovalRequest, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(tagsRemovalRequest);

            // Act
            var isValid = Validator.TryValidateObject(tagsRemovalRequest, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetTagsRemovalRequests()
        {
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = null, Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = null }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int>(), Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string>() }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11" } }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "tag1", "" } }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { ApiTestsHelper.GenerateString(51) } }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag", "tag", "TAG" } }, false };
            yield return new object[] { new TagsRemovalRequest { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2" } }, true };
        }

        [Theory]
        [MemberData(nameof(GetTagSuggestionsQueries))]
        public void TagSuggestionsQuery_ModelStateValidationTheory(TagSuggestionsQuery query, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(query);

            // Act
            var isValid = Validator.TryValidateObject(query, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetTagSuggestionsQueries()
        {
            yield return new object[] { new TagSuggestionsQuery { UsageTimeSpanInDays = null }, true };
            yield return new object[] { new TagSuggestionsQuery { UsageTimeSpanInDays = -1 }, false };
            yield return new object[] { new TagSuggestionsQuery { UsageTimeSpanInDays = 0 }, false };
            yield return new object[] { new TagSuggestionsQuery { UsageTimeSpanInDays = 1 }, true };
            yield return new object[] { new TagSuggestionsQuery { UsageTimeSpanInDays = 30 }, true };
        }

        [Fact]
        public async Task GetToDoItems_ReturnsOk()
        {
            // Act
            var result = await _controller.GetToDoItemsBySearchQuery(null);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetToDoItemStatuses_ReturnsOk()
        {
            // Act
            var result = await _controller.GetToDoItemStatuses();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateToDoItem_ReturnsCreatedAtAction()
        {
            // Arrange
            _mockToDoItemManager.Setup(s => s.CreateNewTaskAsync(It.IsAny<ToDoItemCreateData>())).ReturnsAsync(new ToDoItem { Id = 1, Status = ToDoItemStatus.Planned });

            // Act
            var result = await _controller.CreateToDoItem(It.IsAny<ToDoItemCreateRequest>());

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task UpdateToDoItem_ReturnsOk()
        {
            // Act
            var result = await _controller.UpdateToDoItem(It.IsAny<int>(), It.IsAny<ToDoItemUpdateRequest>());

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddTags_ReturnsOk_WhenNoExceptionIsThrown()
        {
            // Act
            var result = await _controller.AddTags(new TagsAdditionRequest());

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task RemoveTags_ReturnsOk_WhenNoExceptionIsThrown()
        {
            // Act
            var result = await _controller.RemoveTags(new TagsRemovalRequest());

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}