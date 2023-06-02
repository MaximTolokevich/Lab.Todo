using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Builders;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.BLL.Services.ToDoItemManagers.Options;
using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.BLL.Services.UserServices.Models;
using Lab.Todo.BLL.Tests.Helpers;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Helpers;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lab.Todo.BLL.Tests.ServicesTests.ToDoItemManagersTests
{
    public class TaskFieldsTests
    {
        private readonly Mock<IUnitOfWork> _mockToDoUnitOfWork;
        private readonly Mock<IToDoItemsRepository> _mockToDoItems;
        private readonly IToDoItemManager _toDoItemManager;
        private readonly IMapper _mapper;

        public TaskFieldsTests()
        {
            // Setup
            var user = new User
            {
                Id = "0",
                Email = "aliaksei_sizonenka",
                Name = "aliaksei_sizonenka"
            };

            _mockToDoUnitOfWork = new Mock<IUnitOfWork>();
            var mockUserService = new Mock<IUserService>();
            _mockToDoItems = new Mock<IToDoItemsRepository>();
            var mockTags = new Mock<ITagsRepository>();
            var mockTagUserAssociations = new Mock<IToDoItemTagAssociationsRepository>();
            var mockToDoItemManagerOptions = new Mock<IOptions<ToDoItemManagerOptions>>();
            var mockToDoItemQueryBuilder = new Mock<IToDoItemQueryBuilder>();

            _mockToDoUnitOfWork.Setup(uow => uow.ToDoItems).Returns(_mockToDoItems.Object);
            _mockToDoUnitOfWork.Setup(uow => uow.Tags).Returns(mockTags.Object);
            _mockToDoUnitOfWork.Setup(uow => uow.ToDoItemTagAssociations).Returns(mockTagUserAssociations.Object);
            mockUserService.Setup(us => us.Current).Returns(user);
            mockToDoItemManagerOptions.Setup(options => options.Value).Returns(new ToDoItemManagerOptions { MaximumTagAmount = 5 });
            var mockLogger = Mock.Of<ILogger<ToDoItemManager>>();
            _mapper = BLLTestsHelper.SetupMapper();

            _toDoItemManager = new ToDoItemManager(_mockToDoUnitOfWork.Object, mockUserService.Object, mockToDoItemQueryBuilder.Object, _mapper, mockToDoItemManagerOptions.Object, mockLogger);
        }

        [Fact]
        public void Create_New_Task_Should_Throw_NotFoundException_If_ParentTask_Not_Found()
        {
            // Arrange
            var createdTask = new ToDoItemCreateData
            {
                Title = "new title",
                Description = "descr",
                Duration = TimeSpan.FromHours(13),
                ParentTaskId = 3
            };

            var toDoItemDbEntry = new ToDoItemDbEntry
            {
                Title = "new title",
                Description = "descr",
                Duration = TimeSpan.FromHours(13),
                ParentTaskId = 3
            };
            _mockToDoItems.Setup(item => item.GetByIdAsync((int)createdTask.ParentTaskId, false)).Returns((Task<ToDoItemDbEntry>)null);

            // Act
            Func<Task<ToDoItem>> invokeResult = async () => await _toDoItemManager.CreateNewTaskAsync(createdTask);

            // Assert
            invokeResult
                .Should().Throw<NotFoundException>()
                .WithMessage($"Parent task with Id {createdTask.ParentTaskId} does not exist.");

            _mockToDoUnitOfWork.Verify(uow => uow.ToDoItems.Create(toDoItemDbEntry), Times.Never);
            _mockToDoUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public void Should_Correctly_Map_ToDoItemDbEntryTaskFields_To_ToDoItem()
        {
            // Arrange
            var parentTaskDbEntry = new ToDoItemDbEntry
            {
                Id = 2,
                Title = "Title",
                Description = "Descr"
            };

            var toDoItemDbEntry = new ToDoItemDbEntry
            {
                Duration = TimeSpan.FromHours(3),
                ActualStartTime = DateTime.Today,
                ParentTaskId = parentTaskDbEntry.Id,
                ParentTask = parentTaskDbEntry,
                ToDoItemStatusId = 2,
                Deadline = new DateTime(2020, 10, 10),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var toDoItem = _mapper.Map<ToDoItem>(toDoItemDbEntry);

            // Assert
            var expectedParentTask = new ToDoItem
            {
                Id = 2,
                Title = "Title",
                Description = "Descr"
            };

            var expectedToDoItem = new ToDoItem
            {
                Duration = TimeSpan.FromHours(3),
                ActualStartTime = DateTime.Today,
                ParentTask = expectedParentTask,
                Status = ToDoItemStatus.InProgress,
                Deadline = new DateTime(2020, 10, 10),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            toDoItem.Should().BeEquivalentTo(expectedToDoItem);
        }

        [Fact]
        public void Should_Correctly_Map_ToDoItemUpdateDataTaskFields_To_DbEntry()
        {
            // Arrange
            var toDoItemUpdateData = new ToDoItemUpdateData
            {
                Duration = TimeSpan.FromHours(3),
                ParentTaskId = 3,
                Deadline = new DateTime(2020, 10, 10),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var toDoItemDbEntry = _mapper.Map<ToDoItemDbEntry>(toDoItemUpdateData);

            // Assert
            var expectedToDoItemDbEntry = new ToDoItemDbEntry
            {
                Duration = TimeSpan.FromHours(3),
                ParentTaskId = 3,
                Deadline = new DateTime(2020, 10, 10),
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            toDoItemDbEntry.Should().BeEquivalentTo(expectedToDoItemDbEntry);
        }

        [Fact]
        public void Should_Correctly_Map_ToDoItemCreateDataTaskFields_To_DbEntry()
        {
            // Arrange
            var toDoItemCreateData = new ToDoItemCreateData
            {
                Duration = TimeSpan.FromHours(3),
                ParentTaskId = 5,
                Deadline = null,
                PlannedStartTime = new DateTime(2000, 1, 1)
            };

            // Act
            var toDoItemDbEntry = _mapper.Map<ToDoItemDbEntry>(toDoItemCreateData);

            // Assert
            var expectedToDoItemDbEntry = new ToDoItemDbEntry
            {
                Duration = TimeSpan.FromHours(3),
                ParentTaskId = 5,
                Deadline = null,
                PlannedStartTime = new DateTime(2000, 1, 1)
            };
            toDoItemDbEntry.Should().BeEquivalentTo(expectedToDoItemDbEntry);
        }

        [Fact]
        public void Delete_Task_Should_Throw_ApplicationException_If_Deleted_Task_Has_Child_Tasks()
        {
            // Arrange
            int deletedTaskId = 5;
            bool isAnyChildTasks = true;
            var parentTask = new ToDoItemDbEntry
            {
                Id = deletedTaskId
            };

            _mockToDoItems.Setup(rep => rep.GetByIdAsync(deletedTaskId, false)).ReturnsAsync(parentTask);
            _mockToDoItems.Setup(rep => rep.HasChildTasksAsync(deletedTaskId)).ReturnsAsync(isAnyChildTasks);

            // Act
            Func<Task> invokeResult = async () => await _toDoItemManager.DeleteTaskAsync(deletedTaskId);

            // Assert
            invokeResult
                .Should().Throw<ApplicationException>()
                .WithMessage($"Unable to delete. The task with Id {deletedTaskId} has child tasks.");
        }

        [Theory]
        [MemberData(nameof(Get_Previous_And_New_ToDoItems))]
        public async void Should_Correctly_Set_StartTime_And_EndTime(ToDoItemDbEntry toDoItem, ToDoItemUpdateData updatedToDoItem, DateTime? startTime, DateTime? endTime)
        {
            // Arrange
            int id = 1;
            updatedToDoItem.Tags = new List<string> { "tag1" };
            toDoItem.ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry> { new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "tag2" } } };
            _mockToDoItems.Setup(uow => uow.GetByIdAsync(id, true)).ReturnsAsync(toDoItem);

            // Act
            var newItem = await _toDoItemManager.UpdateExistingTaskAsync(id, updatedToDoItem);

            // Assert
            if (startTime is not null)
            {
                newItem.ActualStartTime.Should().BeCloseTo((DateTime)startTime, 6000);
            }
            else
            {
                newItem.ActualStartTime.Should().BeNull();
            }

            if (endTime is not null)
            {
                newItem.ActualEndTime.Should().BeCloseTo((DateTime)endTime, 6000);
            }
            else
            {
                newItem.ActualEndTime.Should().BeNull();
            }
        }

        public static IEnumerable<object[]> Get_Previous_And_New_ToDoItems()
        {
            yield return new object[] { new ToDoItemDbEntry { ToDoItemStatusId = ToDoItemStatusIds.Planned }, new ToDoItemUpdateData { Status = ToDoItemStatus.InProgress }, DateTime.UtcNow, null };
            yield return new object[] { new ToDoItemDbEntry { ToDoItemStatusId = ToDoItemStatusIds.Paused }, new ToDoItemUpdateData { Status = ToDoItemStatus.Completed }, DateTime.UtcNow, DateTime.UtcNow };
            yield return new object[] { new ToDoItemDbEntry { ToDoItemStatusId = ToDoItemStatusIds.InProgress }, new ToDoItemUpdateData { Status = ToDoItemStatus.Completed }, DateTime.UtcNow, DateTime.UtcNow };
            yield return new object[] { new ToDoItemDbEntry { ToDoItemStatusId = ToDoItemStatusIds.Planned }, new ToDoItemUpdateData { Status = ToDoItemStatus.Completed }, DateTime.UtcNow, DateTime.UtcNow };
        }
    }
}