using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Builders;
using Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.BLL.Services.ToDoItemManagers.Options;
using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.BLL.Services.UserServices.Models;
using Lab.Todo.BLL.Tests.Helpers;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lab.Todo.BLL.Tests.ServicesTests.ToDoItemManagersTests
{
    public class UserPermissionTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IToDoItemsRepository> _mockToDoItems;

        private readonly IToDoItemManager _toDoItemManager;

        public UserPermissionTests()
        {
            var user = new User
            {
                Id = "0",
                Email = "User1@mail.com",
                Name = "User1@mail.com"
            };

            var mockToDoUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserService = new Mock<IUserService>();
            _mockToDoItems = new Mock<IToDoItemsRepository>();
            var mockTags = new Mock<ITagsRepository>();
            var mockTagUserAssociations = new Mock<IToDoItemTagAssociationsRepository>();
            var mockToDoItemManagerOptions = new Mock<IOptions<ToDoItemManagerOptions>>();
            var mockToDoItemQueryBuilder = new Mock<IToDoItemQueryBuilder>();

            mockToDoUnitOfWork.Setup(uow => uow.ToDoItems).Returns(_mockToDoItems.Object);
            mockToDoUnitOfWork.Setup(uow => uow.Tags).Returns(mockTags.Object);
            mockToDoUnitOfWork.Setup(uow => uow.ToDoItemTagAssociations).Returns(mockTagUserAssociations.Object);
            _mockUserService.Setup(us => us.Current).Returns(user);
            mockToDoItemManagerOptions.Setup(options => options.Value).Returns(new ToDoItemManagerOptions { MaximumTagAmount = 5, MaximumDependencyAmount = 3 });
            var mapper = BLLTestsHelper.SetupMapper();
            var mockLogger = Mock.Of<ILogger<ToDoItemManager>>();

            _toDoItemManager = new ToDoItemManager(mockToDoUnitOfWork.Object, _mockUserService.Object, mockToDoItemQueryBuilder.Object, mapper, mockToDoItemManagerOptions.Object, mockLogger);
        }

        [Fact]
        public async Task CreateNewTask_Should_Set_CurrentUser_Value_To_AssignedTo_Field_AssignedTo_Is_Null()
        {
            // Arrange
            var toDoItemCreateData = new ToDoItemCreateData
            {
                AssignedTo = null
            };

            // Act
            var returnedToDoItem = await _toDoItemManager.CreateNewTaskAsync(toDoItemCreateData);

            // Assert
            returnedToDoItem.AssignedTo
                .Should().Be(_mockUserService.Object.Current.Email);
        }

        [Theory]
        [MemberData(nameof(GetIsCurrentUserCreatorOfTask))]
        public async Task UpdateExistingTaskAsync_Should_Succeed_When_CalledByPermittedUser(bool currentUserIsCreator)
        {
            // Arrange
            var toDoItemDbEntry = GetToDoItemDbEntry(currentUserIsCreator);

            var toDoItemUpdateData = new ToDoItemUpdateData
            {
                Title = "new title",
                AssignedTo = toDoItemDbEntry.AssignedTo
            };
            _mockToDoItems.Setup(rep => rep.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(toDoItemDbEntry);

            // Act
            Func<Task> result = async () => await _toDoItemManager.UpdateExistingTaskAsync(It.IsAny<int>(), toDoItemUpdateData);

            // Assert
            await result.Should().NotThrowAsync();
        }

        [Theory]
        [MemberData(nameof(GetIsCurrentUserCreatorOfTask))]
        public async Task UpdateExistingTaskAsync_Should_ThrowUnauthorizedUserException_When_CalledByUnpermittedUser(bool currentUserIsCreator)
        {
            // Arrange
            var toDoItemDbEntry = GetToDoItemDbEntry(currentUserIsCreator, "DIFFERENTUSER");
            toDoItemDbEntry.Id = 1;

            var toDoItemUpdateData = new ToDoItemUpdateData
            {
                Title = "new title",
            };
            _mockToDoItems.Setup(rep => rep.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(toDoItemDbEntry);

            // Act
            Func<Task> result = async () => await _toDoItemManager.UpdateExistingTaskAsync(1, toDoItemUpdateData);

            // Assert
            await result.Should().ThrowAsync<UnauthorizedUserException>().WithMessage("Task with Id 1 can't be accessed by current user.");
        }

        [Theory]
        [MemberData(nameof(GetIsCurrentUserCreatorOfTask))]
        public async Task DeleteTaskAsync_Should_DeleteSuccessfully_When_DeletedByPermittedUser(bool currentUserCreatorOfTask)
        {
            // Arrange
            var toDoItemDbEntry = GetToDoItemDbEntry(currentUserCreatorOfTask);
            toDoItemDbEntry.ToDoItemTagAssociations = new();
            _mockToDoItems.Setup(rep => rep.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(toDoItemDbEntry);
            _mockToDoItems.Setup(rep => rep.HasChildTasksAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            Func<Task> result = async () => await _toDoItemManager.DeleteTaskAsync(1);

            // Assert
            await result.Should().NotThrowAsync();
        }


        [Theory]
        [MemberData(nameof(GetIsCurrentUserCreatorOfTask))]
        public async Task DeleteTaskAsync_Should_ThrowUnauthorizedUserException_When_CalledByUnpermittedUser(bool currentUserIsCreator)
        {
            // Arrange
            var toDoItemDbEntry = GetToDoItemDbEntry(currentUserIsCreator, "DIFFERENTUSER");
            toDoItemDbEntry.Id = 1;
            toDoItemDbEntry.ToDoItemTagAssociations = new();

            _mockToDoItems.Setup(rep => rep.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(toDoItemDbEntry);

            // Act
            Func<Task> result = async () => await _toDoItemManager.DeleteTaskAsync(It.IsAny<int>());

            // Assert
            await result.Should().ThrowAsync<UnauthorizedUserException>().WithMessage("Task with Id 1 can't be accessed by current user.");
        }

        [Fact]
        public async Task AddTagsAsync_Should_Succeed_When_CurrentUserIsPermitted_ToAllTasks()
        {
            // Arrange
            var username = _mockUserService.Object.Current.Email;
            var tagsToAdd = new List<string> { "Tag4", "Tag5", "Tag6" };
            var taskItemsIds = new List<int> { 1, 2 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } }
                    },
                    CreatedBy = username
                },
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} }
                    },
                    AssignedTo = username
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> result = async () => await _toDoItemManager.AddTagsAsync(taskItemsIds, tagsToAdd);

            // Assert
            await result.Should().NotThrowAsync();
        }

        [Fact]
        public async Task AddTagsAsync_Should_ThrowUnauthorizedUserException_When_CurrentUserIsNotPermitted_ToOneOfTheTasks()
        {
            // Arrange
            var tagsToAdd = new List<string> { "Tag4", "Tag5", "Tag6" };
            var taskItemsIds = new List<int> { 1, 2 };

            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    Id = 1,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } }
                    },
                    CreatedBy = _mockUserService.Object.Current.Email,
                    AssignedTo = "DIFFERENTUSER"
                },
                new ToDoItemDbEntry
                {
                    Id = 2,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} }
                    },
                    CreatedBy = "DIFFERENTUSER",
                    AssignedTo = "DIFFERENTUSER"
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> result = async () => await _toDoItemManager.AddTagsAsync(taskItemsIds, tagsToAdd);

            // Assert
            await result.Should().ThrowAsync<UnauthorizedUserException>().WithMessage("Task with Id 2 can't be accessed by current user.");
        }

        [Fact]
        public async Task AddTagsAsync_Should_ThrowUnauthorizedUserException_When_CurrentUserIsNotPermitted_ToMultipleTasks()
        {
            // Arrange
            var tagsToAdd = new List<string> { "Tag4", "Tag5", "Tag6" };
            var taskItemsIds = new List<int> { 1, 2 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    Id = 1,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } }
                    },
                    CreatedBy = "DIFFERENTUSER",
                    AssignedTo = "DEFFERENTUSER"
                },
                new ToDoItemDbEntry
                {
                    Id = 2,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} }
                    },
                    CreatedBy = "DIFFERENTUSER",
                    AssignedTo = "DEFFERENTUSER"
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> result = async () => await _toDoItemManager.AddTagsAsync(taskItemsIds, tagsToAdd);

            // Assert
            await result.Should().ThrowAsync<UnauthorizedUserException>().WithMessage("Tasks with Ids 1, 2 can't be accessed by current user.");
        }

        [Fact]
        public async Task RemoveTagsAsync_Should_Succeed_When_CurrentUserIsPermitted_ToAllTasks()
        {
            // Arrange
            var username = _mockUserService.Object.Current.Email;
            var tagsToRemove = new List<string> { "Tag1", "Tag5", "Tag2" };
            var taskItemsIds = new List<int> { 1, 2 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag3" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2" } }
                    },
                    CreatedBy = username
                },
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5"} }
                    },
                    AssignedTo = username
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> result = async () => await _toDoItemManager.RemoveTagsAsync(taskItemsIds, tagsToRemove);

            // Assert
            await result.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RemoveTagsAsync_Should_ThrowUnauthorizedUserException_When_CurrentUserIsNotPermitted_ToOneOfTheTasks()
        {
            // Arrange
            var tagsToRemove = new List<string> { "Tag1", "Tag5", "Tag2" };
            var taskItemsIds = new List<int> { 1, 2 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    Id = 1,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag3" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2" } }
                    },
                    CreatedBy = _mockUserService.Object.Current.Email,
                    AssignedTo = "DEFFERENTUSER"
                },
                new ToDoItemDbEntry
                {
                    Id = 2,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5"} }
                    },
                    CreatedBy = "DIFFERENTUSER",
                    AssignedTo = "DEFFERENTUSER"
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> result = async () => await _toDoItemManager.RemoveTagsAsync(taskItemsIds, tagsToRemove);

            // Assert
            await result.Should().ThrowAsync<UnauthorizedUserException>().WithMessage("Task with Id 2 can't be accessed by current user.");
        }

        [Fact]
        public async Task RemoveTagsAsync_Should_ThrowUnauthorizedUserException_When_CurrentUserIsNotPermitted_ToMultipleTasks()
        {
            // Arrange
            var tagsToRemove = new List<string> { "Tag1", "Tag5", "Tag2" };
            var taskItemsIds = new List<int> { 1, 2 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    Id = 1,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag3" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2" } }
                    },
                    CreatedBy = "DIFFERENTUSER",
                    AssignedTo = "DEFFERENTUSER"
                },
                new ToDoItemDbEntry
                {
                    Id = 2,
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5"} }
                    },
                    CreatedBy = "DIFFERENTUSER",
                    AssignedTo = "DEFFERENTUSER"
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> result = async () => await _toDoItemManager.RemoveTagsAsync(taskItemsIds, tagsToRemove);

            // Assert
            await result.Should().ThrowAsync<UnauthorizedUserException>().WithMessage("Tasks with Ids 1, 2 can't be accessed by current user.");
        }

        [Fact]
        public async Task Should_Throw_PermissionsException_When_Editor_Of_AssignedTo_Field_Is_Not_Creator_Of_Task()
        {
            // Arrange
            var toDoItemDbEntry = GetToDoItemDbEntry(false);

            var toDoItemUpdateData = new ToDoItemUpdateData
            {
                AssignedTo = "DEFFERENTUSER"
            };
            _mockToDoItems.Setup(rep => rep.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(toDoItemDbEntry);

            // Act
            Func<Task> result = async () => await _toDoItemManager.UpdateExistingTaskAsync(It.IsAny<int>(), toDoItemUpdateData);

            // Assert
            await result.Should()
                .ThrowAsync<PermissionsException>()
                .WithMessage("Only creator of the task is able to set AssignedTo field");
        }

        public static IEnumerable<object[]> GetIsCurrentUserCreatorOfTask()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }

        private ToDoItemDbEntry GetToDoItemDbEntry(bool currentUserCreatorOfTask, string currentUserEmail = null)
        {
            currentUserEmail ??= _mockUserService.Object.Current.Email;

            if (currentUserCreatorOfTask)
            {
                return new ToDoItemDbEntry { CreatedBy = currentUserEmail, AssignedTo = "performer" };
            }

            return new ToDoItemDbEntry { AssignedTo = currentUserEmail, CreatedBy = "creator" };
        }
    }
}