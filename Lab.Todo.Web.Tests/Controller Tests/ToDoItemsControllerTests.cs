using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.Web.Controllers;
using Lab.Todo.Web.Models;
using Lab.Todo.Web.Tests.HelpersTests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ToDoItemStatus = Lab.Todo.BLL.Services.ToDoItemManagers.Enums.ToDoItemStatus;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Lab.Todo.Web.Tests.ControllerTests
{
    public class ToDoItemsControllerTests
    {
        private readonly Mock<IToDoItemManager> _mockToDoItemManager;
        private readonly ToDoItemsController _controller;

        public ToDoItemsControllerTests()
        {
            var mapper = WebTestsHelper.SetupMapper();
            _mockToDoItemManager = new Mock<IToDoItemManager>();
            var mockLogger = Mock.Of<ILogger<ToDoItemsController>>();

            _controller = new ToDoItemsController(_mockToDoItemManager.Object, mapper, mockLogger);
        }

        [Theory]
        [MemberData(nameof(GetItemCreateModels))]
        public void ToDoItemCreateModel_ModelStateValidationTheory(ToDoItemCreateModel item, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(item);
            var dependencyViewModelValidationContext = new ValidationContext(item.DependencyViewModel ?? new ToDoItemAddDependencyViewModel());
            var tagViewModelValidationContext = new ValidationContext(item.TagViewModel);

            // Act
            var isValid = Validator.TryValidateObject(item, validationContext, null, true);
            var isInnerDependencyViewModelValid = item.DependencyViewModel is null || Validator.TryValidateObject(item.DependencyViewModel, dependencyViewModelValidationContext, null, true);
            var isInnerTagViewModelValid = Validator.TryValidateObject(item.TagViewModel, tagViewModelValidationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid & isInnerDependencyViewModelValid & isInnerTagViewModelValid);
        }

        public static IEnumerable<object[]> GetItemCreateModels()
        {
            yield return new object[] { new ToDoItemCreateModel { Title = "", Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = null, Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description" }, true };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "" }, true };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = null }, true };
            yield return new object[] { new ToDoItemCreateModel { Title = WebTestsHelper.GenerateString(251), Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = WebTestsHelper.GenerateString(251), Description = "Description" }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "tag1", "" } } }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { WebTestsHelper.GenerateString(51) } } }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "tag1", "tag2", "urgent" } } }, true };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "tag1", "tag2", "tag1" } } }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "Tag", "tag", "TAG" } } }, false };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", DependencyViewModel = new ToDoItemAddDependencyViewModel { DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } } }, true };
            yield return new object[] { new ToDoItemCreateModel { Title = "Title", Description = "Description", DependencyViewModel = new ToDoItemAddDependencyViewModel { DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } } }, false };
        }

        [Theory]
        [MemberData(nameof(GetItemUpdateModels))]
        public void ToDoItemUpdateModel_ModelStateValidationTheory(ToDoItemUpdateModel item, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(item);
            var dependencyViewModelValidationContext = new ValidationContext(item.DependencyViewModel ?? new ToDoItemAddDependencyViewModel());
            var tagViewModelValidationContext = new ValidationContext(item.TagViewModel);

            // Act
            var isValid = Validator.TryValidateObject(item, validationContext, null, true);
            var isInnerDependencyViewModelValid = item.DependencyViewModel is null || Validator.TryValidateObject(item.DependencyViewModel, dependencyViewModelValidationContext, null, true);
            var isInnerTagViewModelValid = Validator.TryValidateObject(item.TagViewModel, tagViewModelValidationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid & isInnerDependencyViewModelValid & isInnerTagViewModelValid);
        }

        public static IEnumerable<object[]> GetItemUpdateModels()
        {
            yield return new object[] { new ToDoItemUpdateModel { Title = "", Description = "Description", Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = null, Description = "Description", Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "", Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateModel { Title = WebTestsHelper.GenerateString(251), Description = "Description", Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "tag1", "" } }, Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { WebTestsHelper.GenerateString(51) } }, Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "tag1", "tag2", "urgent" } }, Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "tag1", "tag2", "tag1" } }, Status = ToDoItemStatus.Planned }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", TagViewModel = new TagViewModel { Tags = new List<string> { "Tag", "tag", "TAG" } } }, false };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", DependencyViewModel = new ToDoItemAddDependencyViewModel { DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } }, Status = ToDoItemStatus.Planned }, true };
            yield return new object[] { new ToDoItemUpdateModel { Title = "Title", Description = "Description", DependencyViewModel = new ToDoItemAddDependencyViewModel { DependsOnItems = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } }, Status = ToDoItemStatus.Planned }, false };
        }

        [Theory]
        [MemberData(nameof(GetTagsAdditionModels))]
        public void TagsAdditionModel_ModelStateValidationTheory(TagsAdditionModel tagsAdditionModel, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(tagsAdditionModel);

            // Act
            var isValid = Validator.TryValidateObject(tagsAdditionModel, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetTagsAdditionModels()
        {
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = null, Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = null }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int>(), Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string>() }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11" } }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "tag1", "" } }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { WebTestsHelper.GenerateString(51) } }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag", "tag", "TAG" } }, false };
            yield return new object[] { new TagsAdditionModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2" } }, true };
        }

        [Theory]
        [MemberData(nameof(GetTagsRemovalModels))]
        public void TagsRemovalModel_ModelStateValidationTheory(TagsRemovalModel tagsRemovalModel, bool expectedResult)
        {
            // Arrange         
            var validationContext = new ValidationContext(tagsRemovalModel);

            // Act
            var isValid = Validator.TryValidateObject(tagsRemovalModel, validationContext, null, true);

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        public static IEnumerable<object[]> GetTagsRemovalModels()
        {
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = null, Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = null }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int>(), Tags = new List<string> { "Tag1", "Tag2" } }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string>() }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6", "Tag7", "Tag8", "Tag9", "Tag10", "Tag11" } }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "tag1", "" } }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { WebTestsHelper.GenerateString(51) } }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag", "tag", "TAG" } }, false };
            yield return new object[] { new TagsRemovalModel { ToDoItemIds = new List<int> { 1, 2, 3 }, Tags = new List<string> { "Tag1", "Tag2" } }, true };
        }

        [Fact]
        public async Task Should_GetAllTasks_RedirectToViewToDoItems()
        {
            // Arrange
            _mockToDoItemManager.Setup(s => s.GetAllTasksAsync(null)).ReturnsAsync(It.IsAny<List<ToDoItem>>());
            
            // Act
            var result = await _controller.ViewToDoItems(It.IsAny<ToDoItemSearchModel>()) as ViewResult;

            // Assert
            _mockToDoItemManager.Verify(manager => manager.GetAllTasksAsync(null));
            result.ViewData.Model.Should().BeOfType<TaskListViewModel>();
        }

        [Fact]
        public async Task Should_GetAllStatuses_RedirectToViewToDoStatuses()
        {
            // Arrange
            _mockToDoItemManager.Setup(s => s.GetAllStatusesAsync()).ReturnsAsync(It.IsAny<List<ToDoItemStatus>>());

            // Act
            var result = await _controller.ViewToDoItemStatuses() as ViewResult;

            // Assert
            _mockToDoItemManager.Verify(manager => manager.GetAllStatusesAsync());
            result.ViewData.Model.Should().BeOfType<List<ToDoItemStatusModel>>();
        }

        [Fact]
        public async Task Should_CreateNewTaskInDb_RedirectToViewToDoItems_When_PassedValidModel_ToDoItemCreateRequest()
        {
            // Arrange
            var createRequest = new ToDoItemCreateModel { Title = "Title", Description = "Description" };
            var returnedTask = new ToDoItem
            {
                Id = 0,
                Title = createRequest.Title,
                Description = createRequest.Description,
                CreatedDate = DateTime.UtcNow,
                Status = ToDoItemStatus.Planned
            };
            _mockToDoItemManager.Setup(s => s.CreateNewTaskAsync(It.IsAny<ToDoItemCreateData>())).ReturnsAsync(returnedTask);

            // Act
            var result = await _controller.Add(createRequest) as RedirectToActionResult;

            // Assert
            _mockToDoItemManager.Verify(manager => manager.CreateNewTaskAsync(It.IsAny<ToDoItemCreateData>()));
            result.ActionName.Should().Be(nameof(_controller.ViewToDoItems));
        }

        [Fact]
        public async Task Should_GetTaskFromDb_OpenViewEdit_When_PassedValidId()
        {
            // Arrange
            var returnedTask = new ToDoItem { Id = 1, Title = "Title", Status = ToDoItemStatus.Planned };
            _mockToDoItemManager.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(returnedTask);

            // Act
            _ = await _controller.Edit(1) as ViewResult;

            // Assert
            _mockToDoItemManager.Verify(manager => manager.GetTaskByIdAsync(1, It.IsAny<bool>()));
        }

        [Fact]
        public async Task Should_RedirectToErrorView_When_PassedInvalidTaskId()
        {
            // Arrange
            _mockToDoItemManager.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new ApplicationException());

            // Act
            Func<Task> result = async () => await _controller.Edit(69);

            // Assert
            await result
                .Should().ThrowAsync<ApplicationException>();

            _mockToDoItemManager.Verify(manager => manager.GetTaskByIdAsync(69, It.IsAny<bool>()));
        }

        [Fact]
        public async Task Should_UpdateTaskInDb_RedirectToViewToDoItems_When_PassedValidModel_ToDoItemUpdateRequest()
        {
            // Arrange
            var toDoItemModel = new ToDoItemUpdateModel { Id = 1, Title = "Title", Status = ToDoItemStatus.Planned };
            _mockToDoItemManager.Setup(s => s.UpdateExistingTaskAsync(It.IsAny<int>(), It.IsAny<ToDoItemUpdateData>())).ReturnsAsync(It.IsAny<ToDoItem>);

            // Act
            var result = await _controller.Edit(toDoItemModel) as RedirectToActionResult;

            // Assert
            result.ActionName.Should().Be(nameof(_controller.ViewToDoItems));
        }

        [Fact]
        public async Task UpdateToDoItem_RedirectsToEditView_WhenCyclicDependencyExceptionIsThrown()
        {
            // Assert
            var cyclicDependencies = new List<(int, IEnumerable<int>)>() { (3, new List<int>() { 5, 3, 2, 5 }) };
            var toDoItemUpdateModel = new ToDoItemUpdateModel() { Status = ToDoItemStatus.Planned, DependencyViewModel = new ToDoItemAddDependencyViewModel() };
            var toDoItems = new List<ToDoItem> { new ToDoItem { Id = 2 }, new ToDoItem { Id = 3 }, new ToDoItem { Id = 5 } };

            _mockToDoItemManager.Setup(s => s.UpdateExistingTaskAsync(It.IsAny<int>(), It.IsAny<ToDoItemUpdateData>()))
                .ThrowsAsync(new CyclicDependencyException { CyclicDependencies = cyclicDependencies });
            _mockToDoItemManager.Setup(s => s.GetAllTasksAsync(null)).ReturnsAsync(toDoItems);

            // Act
            var result = await _controller.Edit(toDoItemUpdateModel) as ViewResult;

            // Assert
            var dependecies = new List<DependencyModel>
            {
                new DependencyModel { ToDoItemId = 2 },
                new DependencyModel { ToDoItemId = 3 },
                new DependencyModel { ToDoItemId = 5 }
            };

            var expectedCyclicDependencies = new List<(DependencyModel Source, List<DependencyModel> Cycle)>
            {
                (dependecies[1], new List<DependencyModel> { dependecies[2], dependecies[1], dependecies[0], dependecies[2] })
            };

            ((ToDoItemUpdateModel)result.Model).DependencyViewModel.CyclicDependencies
                .Should().BeEquivalentTo(expectedCyclicDependencies);
        }
    }
}