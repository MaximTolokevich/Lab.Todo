using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ToDoItemManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockToDoUnitOfWork;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IToDoItemsRepository> _mockToDoItems;
        private readonly Mock<ITagsRepository> _mockTags;
        private readonly Mock<IToDoItemTagAssociationsRepository> _mockTagUserAssociations;
        private readonly Mock<IToDoItemDependenciesRepository> _mockDependencies;
        private readonly Mock<IToDoItemQueryBuilder> _mockToDoItemQueryBuilder;
        private readonly IToDoItemManager _toDoItemManager;
        private readonly IMapper _mapper;

        public ToDoItemManagerTests()
        {
            // Setup
            var user = new User
            {
                Id = "0",
                Email = "test_user@mail.com",
                Name = "test_user@mail.com"
            };

            _mockToDoUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserService = new Mock<IUserService>();
            _mockToDoItems = new Mock<IToDoItemsRepository>();
            _mockTags = new Mock<ITagsRepository>();
            _mockTagUserAssociations = new Mock<IToDoItemTagAssociationsRepository>();
            var mockCustomFields = new Mock<ICustomFieldsRepository>();
            _mockDependencies = new Mock<IToDoItemDependenciesRepository>();
            var mockToDoItemManagerOptions = new Mock<IOptions<ToDoItemManagerOptions>>();
            _mockToDoItemQueryBuilder = new Mock<IToDoItemQueryBuilder>();

            _mockToDoUnitOfWork.Setup(uow => uow.ToDoItems).Returns(_mockToDoItems.Object);
            _mockToDoUnitOfWork.Setup(uow => uow.CustomFields).Returns(mockCustomFields.Object);
            _mockToDoUnitOfWork.Setup(uow => uow.Tags).Returns(_mockTags.Object);
            _mockToDoUnitOfWork.Setup(uow => uow.ToDoItemTagAssociations).Returns(_mockTagUserAssociations.Object);
            _mockToDoUnitOfWork.Setup(uow => uow.ToDoItemDependencies).Returns(_mockDependencies.Object);
            _mockUserService.Setup(us => us.Current).Returns(user);
            mockToDoItemManagerOptions.Setup(options => options.Value).Returns(new ToDoItemManagerOptions { MaximumTagAmount = 5, MaximumDependencyAmount = 3 });
            _mapper = BLLTestsHelper.SetupMapper();
            var mockLogger = Mock.Of<ILogger<ToDoItemManager>>();

            _toDoItemManager = new ToDoItemManager(_mockToDoUnitOfWork.Object, _mockUserService.Object, _mockToDoItemQueryBuilder.Object, _mapper, mockToDoItemManagerOptions.Object, mockLogger);
        }

        [Fact]
        public async void Should_ReturnNewCreatedTask_When_PassedFilledNewToDoItemInfo()
        {
            // Arrange
            var toDoItemCreateData = new ToDoItemCreateData
            {
                Title = "New Title",
                Description = "New Description",
                PlannedStartTime = new DateTime(2000, 1, 1),
                Tags = new List<string> { "life", "Urgent", "sPoRt" },
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData> {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 1 },
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 4 },
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 7 },
                }
            };

            var toDoItemsIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _mockToDoItems.Setup(rep => rep.Create(It.IsAny<ToDoItemDbEntry>()));
            _mockToDoItems.Setup(rep => rep.GetAllIdsAsync()).ReturnsAsync(toDoItemsIds);
            _mockTags.Setup(rep => rep.GetByValuesAsync(It.IsAny<IEnumerable<string>>(), true)).ReturnsAsync(new List<TagDbEntry>());

            // Act
            var returnedTask = await _toDoItemManager.CreateNewTaskAsync(toDoItemCreateData);

            // Assert
            var expectedTask = new ToDoItem()
            {
                Id = 0,
                Title = toDoItemCreateData.Title,
                Description = toDoItemCreateData.Description,
                Tags = new List<string> { "Life", "Urgent", "Sport" },
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _mockUserService.Object.Current.Email,
                AssignedTo = _mockUserService.Object.Current.Email,
                Status = ToDoItemStatus.Planned,
                PlannedStartTime = new DateTime(2000, 1, 1),
                DependsOnItems = new List<ToDoItemDependency> {
                    new ToDoItemDependency { ToDoItemId = 1},
                    new ToDoItemDependency { ToDoItemId = 4},
                    new ToDoItemDependency { ToDoItemId = 7},
                }
            };

            returnedTask
                .Should().BeEquivalentTo(expectedTask, options =>
                options.Excluding(f => f.CreatedDate));

            returnedTask.CreatedDate
                .Should().BeCloseTo(expectedTask.CreatedDate, 1000);
            returnedTask.Status.Should().BeEquivalentTo(ToDoItemStatus.Planned);
            _mockToDoUnitOfWork.Verify(uow => uow.SaveChangesAsync());
        }

        [Fact]
        public async void Should_ReturnUpdatedExistingTask_When_PassedFilledToDoItemUpdateInfo()
        {
            // Arrange
            var taskId = 12;
            var toDoItemUpdateData = new ToDoItemUpdateData
            {
                Title = "New Title",
                Description = "New Description",
                Tags = new List<string> { "Tag2", "Tag3" },
                Status = ToDoItemStatus.Completed,
                PlannedStartTime = new DateTime(2000, 1, 1),
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData> {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 1 },
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 4 },
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 7 },
                }
            };
            var initialTask = new ToDoItemDbEntry()
            {
                Id = 12,
                Title = "OLD TITLE",
                Description = "OLD DESCRIPTION",
                ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                {
                    new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                    new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2" } }
                },
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test_user@mail.com"
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskId, true)).ReturnsAsync(initialTask);
            var toDoItemsIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _mockToDoItems.Setup(rep => rep.GetAllIdsAsync()).ReturnsAsync(toDoItemsIds);


            // Act
            var returnedUpdatedTask = await _toDoItemManager.UpdateExistingTaskAsync(taskId, toDoItemUpdateData);
            returnedUpdatedTask.ActualStartTime = returnedUpdatedTask.ActualEndTime = null;
            returnedUpdatedTask.Duration = null;

            // Assert
            var expectedTask = new ToDoItem()
            {
                Id = 12,
                Title = toDoItemUpdateData.Title,
                Description = toDoItemUpdateData.Description,
                Tags = new List<string> { "Tag2", "Tag3" },
                CreatedDate = initialTask.CreatedDate,
                CreatedBy = initialTask.CreatedBy,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = _mockUserService.Object.Current.Email,
                Status = ToDoItemStatus.Completed,
                PlannedStartTime = new DateTime(2000, 1, 1),
                DependsOnItems = new List<ToDoItemDependency> {
                    new ToDoItemDependency { ToDoItemId = 1},
                    new ToDoItemDependency { ToDoItemId = 4},
                    new ToDoItemDependency { ToDoItemId = 7},
                }
            };

            returnedUpdatedTask
                .Should().BeEquivalentTo(expectedTask, options => options
                .Excluding(s => s.ModifiedDate));

            returnedUpdatedTask.ModifiedDate
                .Should().BeCloseTo((DateTime)expectedTask.ModifiedDate, 1100);

            _mockToDoUnitOfWork.Verify(uow => uow.SaveChangesAsync());
            _mockToDoUnitOfWork.Verify(uow => uow.ToDoItemTagAssociations.Create(It.IsAny<ToDoItemTagAssociationDbEntry>()), Times.Exactly(1));
        }

        [Fact]
        public async void Should_ThrowNotFoundException_When_PassingInvalidTaskId_UpdateExistingTaskAsync()
        {
            // Arrange
            var taskId = 12;
            var toDoItemUpdateData = new ToDoItemUpdateData { Title = "New Title", Description = "New Description", Status = ToDoItemStatus.Completed };
            var initialTask = new ToDoItemDbEntry()
            {
                Id = 0,
                Title = "OLD TITLE",
                Description = "OLD DESCRIPTION",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Username",
                ToDoItemStatusId = 1
            };
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskId, false)).ReturnsAsync((ToDoItemDbEntry)null);

            // Act
            Func<Task<ToDoItem>> updateInvokeResult = async () => await _toDoItemManager.UpdateExistingTaskAsync(taskId, toDoItemUpdateData);

            // Assert
            await updateInvokeResult
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Task with Id {taskId} does not exist.");

            _mockToDoUnitOfWork.Verify(uow => uow.ToDoItems.Update(initialTask), Times.Never);
            _mockToDoUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async void Should_DeleteExistingTask()
        {
            // Arrange
            var taskId = 5;
            var task = new ToDoItemDbEntry()
            {
                Id = taskId,
                Title = "Title",
                Description = "Description",
                ToDoItemTagAssociations = new(),
                CreatedBy = _mockUserService.Object.Current.Email,
                CreatedDate = DateTime.UtcNow,
                ToDoItemStatusId = 1,
                DependsOnItems = null,
                DependentItems = null,
                CustomFields = new List<CustomFieldDbEntry> { It.IsAny<CustomFieldDbEntry>() }
            };
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskId, false)).ReturnsAsync(task);

            // Act
            await _toDoItemManager.DeleteTaskAsync(taskId);

            // Assert
            _mockToDoUnitOfWork.Verify(uow => uow.ToDoItems.Delete(taskId));
            _mockToDoUnitOfWork.Verify(uow => uow.CustomFields.DeleteRelatedToToDoItem(taskId));
            _mockToDoUnitOfWork.Verify(uow => uow.SaveChangesAsync());
        }

        [Fact]
        public async void Should_ThrowNotFoundException_When_PassingInvalidTaskId_DeleteTaskAsync()
        {
            // Arrange
            var taskId = 5;
            _mockToDoUnitOfWork.Setup(uow => uow.ToDoItems.GetByIdAsync(taskId, false)).ReturnsAsync((ToDoItemDbEntry)null);

            // Act
            Func<Task> deleteInvokeResult = async () => await _toDoItemManager.DeleteTaskAsync(taskId);

            // Assert
            await deleteInvokeResult
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Task with Id {taskId} does not exist.");

            _mockToDoUnitOfWork.Verify(uow => uow.CustomFields.DeleteRelatedToToDoItem(taskId), Times.Never);
            _mockToDoUnitOfWork.Verify(uow => uow.ToDoItems.Delete(taskId), Times.Never);
            _mockToDoUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async void Should_ReturnMapped_ToDoItem()
        {
            // Arrange
            var taskId = 5;
            var task = new ToDoItemDbEntry()
            {
                Id = 5,
                Title = "TITLE",
                Description = "DESCRIPTION",
                ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                {
                    new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                },
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _mockUserService.Object.Current.Email,
                ToDoItemStatusId = 1,
                DependsOnItems = new List<ToDoItemDependencyDbEntry> {
                    new ToDoItemDependencyDbEntry{ ToDoItemId = taskId, DependsOnToDoItemId = 1},
                    new ToDoItemDependencyDbEntry{ ToDoItemId = taskId, DependsOnToDoItemId = 2}
                }
            };
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskId, false)).ReturnsAsync(task);

            // Act
            var returnedTask = await _toDoItemManager.GetTaskByIdAsync(taskId);

            // Assert
            var expectedReturnedTask = new ToDoItem()
            {
                Id = 5,
                Title = task.Title,
                Description = task.Description,
                Tags = new List<string> { "Tag1" },
                CreatedDate = task.CreatedDate,
                CreatedBy = task.CreatedBy,
                Status = ToDoItemStatus.Planned,
                DependsOnItems = new List<ToDoItemDependency> {
                    new ToDoItemDependency{ToDoItemId = 1},
                    new ToDoItemDependency{ToDoItemId = 2}
                }
            };

            returnedTask
                .Should().BeEquivalentTo(expectedReturnedTask);
        }

        [Fact]
        public async void Should_ThrowNotFoundException_When_PassingInvalidTaskId_GetTaskByIdAsync()
        {
            // Arrange
            var taskId = 5;
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskId, false)).ReturnsAsync((ToDoItemDbEntry)null);

            // Act
            Func<Task> getByIdAsyncInvokeResult = async () => await _toDoItemManager.GetTaskByIdAsync(taskId);

            // Assert
            await getByIdAsyncInvokeResult
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Task with Id {taskId} does not exist.");
        }

        [Fact]
        public async void Should_ReturnNull_When_PassingInvalidTaskId_GetTaskByIdAsync()
        {
            // Arrange
            var taskId = 5;
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskId, false)).ReturnsAsync((ToDoItemDbEntry)null);

            // Act
            var returnedTask = await _toDoItemManager.GetTaskByIdAsync(taskId, throwExceptionIfTaskNotFound: false);

            // Assert
            returnedTask
                .Should().BeNull();
        }

        [Fact]
        public async void Should_ReturnCorrectlyMappedFilledToDoItemList()
        {
            // Arrange
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry { Id = 1, Title = "FIRST", Description = "", CreatedDate = DateTime.Today, CreatedBy = "test_user@mail.com", ToDoItemStatusId = 1 },
                new ToDoItemDbEntry { Id = 2, Title = "SECOND", Description = "POGGERS", CreatedDate = DateTime.Today, CreatedBy = "test_user@mail.com", ToDoItemStatusId = 2},
                new ToDoItemDbEntry { Id = 3, Title = "THIRD", Description = "", CreatedDate = DateTime.Today, CreatedBy = "test_user@mail.com", ToDoItemStatusId = 3}
            };

            _mockToDoItems.Setup(repo => repo.GetAllAsQueryable(It.IsAny<bool>())).Returns(tasks.AsQueryable());
            _mockToDoItemQueryBuilder.Setup(builder => builder.SetBaseQuery(It.IsAny<IQueryable<ToDoItemDbEntry>>())).Returns(_mockToDoItemQueryBuilder.Object);
            _mockToDoItemQueryBuilder.Setup(builder => builder.Build()).Returns(tasks.AsQueryable());

            // Act
            var returnedTasks = await _toDoItemManager.GetAllTasksAsync();
            var mappedReturnedTasks = _mapper.Map<IEnumerable<ToDoItem>>(returnedTasks);
            var returnedTasksList = mappedReturnedTasks.ToList();

            // Assert
            var expectedTasks = new List<ToDoItem>
            {
                new ToDoItem { Id = 1, Title = "FIRST", Description = "", CreatedDate = DateTime.Today, CreatedBy = "test_user@mail.com", Status = ToDoItemStatus.Planned },
                new ToDoItem { Id = 2, Title = "SECOND", Description = "POGGERS", CreatedDate = DateTime.Today, CreatedBy = "test_user@mail.com", Status = ToDoItemStatus.InProgress },
                new ToDoItem { Id = 3, Title = "THIRD", Description = "", CreatedDate = DateTime.Today, CreatedBy = "test_user@mail.com", Status = ToDoItemStatus.Paused }
            };

            returnedTasksList
                .Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public async void CreateNewTaskAsync_ThrowsWrongDependencyException_WhenPassedInvalidDependencies()
        {
            // Arrange
            var itemCreateData = new ToDoItemCreateData
            {
                Title = "Task1",
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData> {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 3}
                }
            };
            var toDoItemsIds = new List<int> { 1, 5, 10 };
            _mockToDoItems.Setup(rep => rep.GetAllIdsAsync()).ReturnsAsync(toDoItemsIds);

            // Act
            Func<Task> act = async () => await _toDoItemManager.CreateNewTaskAsync(itemCreateData);

            // Assert
            await act.Should().ThrowAsync<WrongDependencyException>();
        }

        [Fact]
        public async void CreateNewTaskAsync_ThrowsWrongDependencyException_WhenDependencyAmountExceedsLimit()
        {
            // Arrange
            var itemCreateData = new ToDoItemCreateData
            {
                Title = "Task1",
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData> {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 1},
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 2},
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 3},
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 4}
                }
            };
            var toDoItemsIds = new List<int> { 1, 2, 3, 4, 5 };
            _mockToDoItems.Setup(rep => rep.GetAllIdsAsync()).ReturnsAsync(toDoItemsIds);

            // Act
            Func<Task> act = async () => await _toDoItemManager.CreateNewTaskAsync(itemCreateData);

            // Assert
            await act.Should().ThrowAsync<WrongDependencyException>();
        }

        [Fact]
        public async void CreateNewTaskAsync_ThrowsTagDuplicateException_WhenTagsAreNotUnique()
        {
            // Arrange
            var itemCreateData = new ToDoItemCreateData { Title = "Task1", Tags = new List<string> { "Tag1", "Tag2", "Tag1" } };

            // Act
            Func<Task> act = async () => await _toDoItemManager.CreateNewTaskAsync(itemCreateData);

            // Assert
            await act.Should().ThrowAsync<TagDuplicateException>();
        }

        [Fact]
        public async void CreateNewTaskAsync_ThrowsTagLimitException_WhenTagAmountExceedsLimit()
        {
            // Arrange
            var itemCreateData = new ToDoItemCreateData { Title = "Task1", Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6" } };

            // Act
            Func<Task> act = async () => await _toDoItemManager.CreateNewTaskAsync(itemCreateData);

            // Assert
            await act.Should().ThrowAsync<TagLimitException>();
        }

        [Fact]
        public async void UpdateExistingTaskAsync_ThrowsWrongDependencyException_WhenPassedInvalidDependencies()
        {
            // Arrange
            var itemUpdateData = new ToDoItemUpdateData
            {
                Title = "Task1",
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData> {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 3}
                }
            };

            var toDoItemsIds = new List<int> { 1, 5, 10 };
            _mockToDoItems.Setup(rep => rep.GetAllIdsAsync()).ReturnsAsync(toDoItemsIds);
            _mockToDoItems.Setup(rep => rep.GetByIdAsync(1, true)).ReturnsAsync(new ToDoItemDbEntry());

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(1, itemUpdateData);

            // Assert
            await act.Should().ThrowAsync<WrongDependencyException>();
        }

        [Fact]
        public async void UpdateExistingTaskAsync_ThrowsWrongDependencyException_WhenDependencyAmountExceedsLimit()
        {
            // Arrange
            var itemUpdateData = new ToDoItemUpdateData
            {
                Title = "Task1",
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData> {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 2},
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 3},
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 4},
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 5}
                }
            };

            var toDoItemsIds = new List<int> { 1, 2, 3, 4, 5 };
            _mockToDoItems.Setup(rep => rep.GetAllIdsAsync()).ReturnsAsync(toDoItemsIds);
            _mockToDoItems.Setup(rep => rep.GetByIdAsync(1, true)).ReturnsAsync(new ToDoItemDbEntry());

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(1, itemUpdateData);
            // Assert
            await act.Should().ThrowAsync<WrongDependencyException>();
        }

        [Fact]
        public async void UpdateExistingTaskAsync_ThrowsTagDuplicateException_WhenTagsAreNotUnique()
        {
            // Arrange
            var itemUpdateData = new ToDoItemUpdateData { Title = "Task2", Tags = new List<string> { "Tag2", "Tag1", "Tag2" } };
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(1, true)).ReturnsAsync(new ToDoItemDbEntry());

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(1, itemUpdateData);

            // Assert
            await act.Should().ThrowAsync<TagDuplicateException>();
        }

        [Fact]
        public async void UpdateExistingTaskAsync_ThrowsTagLimitException_WhenTagAmountExceedsLimit()
        {
            // Arrange
            var itemUpdateData = new ToDoItemUpdateData { Title = "Task2", Tags = new List<string> { "Tag1", "Tag2", "Tag3", "Tag4", "Tag5", "Tag6" } };
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(1, true)).ReturnsAsync(new ToDoItemDbEntry());

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(1, itemUpdateData);

            // Assert
            await act.Should().ThrowAsync<TagLimitException>();
        }

        [Fact]
        public async void GetTagSuggestionsAsync_ReturnsAvailableTagSuggestions()
        {
            // Arrange
            var tags = new List<TagDbEntry>
            {
                new TagDbEntry { Value = "Urgent" },
                new TagDbEntry { Value = "Sport" },
                new TagDbEntry { Value = "Tag1" },
                new TagDbEntry { Value = "Tag2" }
            };

            _mockTagUserAssociations.Setup(repo => repo.GetMostUsedTagsAsync(It.IsAny<string>(), null, true))
                .ReturnsAsync(new List<TagDbEntry> { tags[2], tags[0], tags[3] });
            _mockTags.Setup(repo => repo.GetPredefinedAsync(true))
                .ReturnsAsync(new List<TagDbEntry> { tags[0], tags[1] });

            // Act
            var availableTagSuggestions = await _toDoItemManager.GetTagSuggestionsAsync();

            // Assert
            var expectedAvailableTags = new List<Tag>
            {
                new Tag { Value = "Tag1" },
                new Tag { Value = "Urgent" },
                new Tag { Value = "Tag2" },
                new Tag { Value = "Sport" }
            };
            availableTagSuggestions.Should().BeEquivalentTo(expectedAvailableTags);
        }

        [Fact]
        public async void AddTags_ReturnsUpdatedTasks()
        {
            // Arrange
            var tagsToAdd = new List<string> { "Tag4", "Tag5", "Tag6" };
            var taskItemsIds = new List<int> { 1, 2 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } }
                    }
                },
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} }
                    }
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            var returnedTasks = await _toDoItemManager.AddTagsAsync(taskItemsIds, tagsToAdd);

            // Assert
            var expectedResult = new List<ToDoItem>
            {
                new ToDoItem { Tags = new List<string> { "Tag1", "Tag4", "Tag5", "Tag6" } },
                new ToDoItem { Tags = new List<string> { "Tag1", "Tag2", "Tag4", "Tag5", "Tag6" } }
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async void AddTags_ThrowsNotFoundException_WhenToDoItemsDoNotExist()
        {
            // Act
            Func<Task> act = async () => await _toDoItemManager.AddTagsAsync(new List<int> { 1, 2 }, It.IsAny<IEnumerable<string>>());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async void AddTags_ThrowsTagLimitException_WhenTagAmountExceedsLimit()
        {
            // Arrange
            var tagsToAdd = new List<string> { "Tag4", "Tag5", "Tag6" };
            var taskItemsIds = new List<int> { 1 };
            var tasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag3" } }
                    }
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> act = async () => await _toDoItemManager.AddTagsAsync(taskItemsIds, tagsToAdd);

            // Assert
            await act.Should().ThrowAsync<TagLimitException>();
        }

        [Fact]
        public async void RemoveTags_ReturnsUpdatedTasks()
        {
            // Arrange
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
                    }
                },
                new ToDoItemDbEntry
                {
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag2"} },
                        new ToDoItemTagAssociationDbEntry { Tag = new TagDbEntry { Value = "Tag5"} }
                    }
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            var returnedTasks = await _toDoItemManager.RemoveTagsAsync(taskItemsIds, tagsToRemove);

            // Assert
            var expectedResult = new List<ToDoItem>
            {
                new ToDoItem { Tags = new List<string> { "Tag3" } },
                new ToDoItem { Tags = new List<string>() }
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async void RemoveTags_ThrowsNotFoundException_WhenToDoItemsDoNotExist()
        {
            // Act
            Func<Task> act = async () => await _toDoItemManager.RemoveTagsAsync(new List<int> { 1, 2 }, It.IsAny<IEnumerable<string>>());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async void RemoveTags_ThrowsExtraTagException_WhenTagsToRemoveHaveExtraTags()
        {
            // Arrange
            var tagsToRemove = new List<string> { "Tag1", "ExtraTag1", "Tag2", "ExtraTag2" };
            var taskItemsIds = new List<int> { 1 };
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
                    }
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(taskItemsIds, It.IsAny<bool>())).ReturnsAsync(tasks);

            // Act
            Func<Task> act = async () => await _toDoItemManager.RemoveTagsAsync(taskItemsIds, tagsToRemove);

            // Assert
            await act.Should().ThrowAsync<ExtraTagException>();
        }

        [Fact]
        public async void UpdateExistingTaskAsync_DoesNotThrowCyclicDependencyException_WhenThereAreNoCycles()
        {
            // Arrange
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new ToDoItemDbEntry());
            _mockDependencies.Setup(repo => repo.GetCyclicDependenciesAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<CyclicDependencyDbEntry>());

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(It.IsAny<int>(), It.IsAny<ToDoItemUpdateData>());

            // Assert
            await act.Should().NotThrowAsync<CyclicDependencyException>();
        }

        [Fact]
        public async void UpdateExistingTaskAsync_ThrowsCyclicDependencyException_WhenThereAreCycles()
        {
            // Arrange
            var cyclicDependencies = new List<CyclicDependencyDbEntry> { new CyclicDependencyDbEntry { IdsCycle = new List<int> { 5, 3, 2, 9, 5 } } };
            var toDoItemUpdateData = new ToDoItemUpdateData()
            {
                DependsOnItems = new List<ToDoItemDependencyCreateUpdateData>()
                {
                    new ToDoItemDependencyCreateUpdateData { ToDoItemId = 3 }
                }
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new ToDoItemDbEntry());
            _mockDependencies.Setup(repo => repo.GetCyclicDependenciesAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(cyclicDependencies);

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(It.IsAny<int>(), toDoItemUpdateData);

            // Assert
            var expectedCyclicDependenciesTuples = new List<(int, IEnumerable<int>)>() { (3, new List<int>() { 5, 3, 2, 9, 5 }) };

            var cyclicDependencyException = (await act.Should().ThrowAsync<CyclicDependencyException>()).Subject.First();
            cyclicDependencyException.CyclicDependencies.Should().BeEquivalentTo(expectedCyclicDependenciesTuples);
        }

        [Fact]
        public async Task UpdateExistingTaskAsync_DoesNotThrowCyclicParentTaskException_WhenThereAreNoCycles()
        {
            // Arrange
            _mockToDoItems.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new ToDoItemDbEntry());
            _mockDependencies.Setup(repo => repo.GetCyclicParentTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<CyclicDependencyDbEntry>());

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(It.IsAny<int>(), It.IsAny<ToDoItemUpdateData>());

            // Assert
            await act.Should().NotThrowAsync<CyclicParentTaskException>();
        }

        [Fact]
        public async Task UpdateExistingTaskAsync_ThrowsCyclicParentTaskException_WhenThereAreCycles()
        {
            // Arrange
            var cyclicParentTasks = new List<CyclicDependencyDbEntry> { new CyclicDependencyDbEntry { IdsCycle = new List<int> { 5, 1, 2, 9, 5 } } };
            var toDoItemUpdateData = new ToDoItemUpdateData()
            {
                ParentTaskId = 2
            };

            _mockToDoItems.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new ToDoItemDbEntry());
            _mockDependencies.Setup(repo => repo.GetCyclicParentTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(cyclicParentTasks);

            // Act
            Func<Task> act = async () => await _toDoItemManager.UpdateExistingTaskAsync(It.IsAny<int>(), toDoItemUpdateData);

            // Assert
            var expectedCyclicParentTasksTuples = new List<(int, IEnumerable<int>)>() { (1, new List<int>() { 5, 1, 2, 9, 5 }) };

            var cyclicDependencyException = (await act.Should().ThrowAsync<CyclicParentTaskException>()).Subject.First();
            cyclicDependencyException.CyclicParentTasks.Should().BeEquivalentTo(expectedCyclicParentTasksTuples);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByTitleItems()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "Title1" },
                new ToDoItemDbEntry() { Title = "Title2" },
                new ToDoItemDbEntry() { Title = "Title3" },
                new ToDoItemDbEntry() { Title = "Invalid1" },
                new ToDoItemDbEntry() { Title = "Invalid2" },
                new ToDoItemDbEntry() { Title = "Invalid3" },
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByTitle("Title")
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "Title1" },
                new ToDoItemDbEntry() { Title = "Title2" },
                new ToDoItemDbEntry() { Title = "Title3" }
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByDescriptionItems()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "Title", Description = "Desc1" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Desc2" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Desc3" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Invalid1" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Invalid2" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Invalid3" },
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByDescription("Desc")
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "Title", Description = "Desc1" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Desc2" },
                new ToDoItemDbEntry() { Title = "Title", Description = "Desc3" }
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByDurationItems()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", Duration = TimeSpan.FromDays(1) },
                new ToDoItemDbEntry() { Title = "ValidTask", Duration = TimeSpan.FromDays(1) },
                new ToDoItemDbEntry() { Title = "ValidTask", Duration = TimeSpan.FromDays(1) },
                new ToDoItemDbEntry() { Title = "InvalidTask", Duration = TimeSpan.FromHours(1) },
                new ToDoItemDbEntry() { Title = "InvalidTask", Duration = TimeSpan.FromDays(4) },
                new ToDoItemDbEntry() { Title = "InvalidTask", Duration = TimeSpan.FromSeconds(1) },

            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByDuration(TimeSpan.FromDays(0.5), TimeSpan.FromDays(1.5))
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", Duration = TimeSpan.FromDays(1) },
                new ToDoItemDbEntry() { Title = "ValidTask", Duration = TimeSpan.FromDays(1) },
                new ToDoItemDbEntry() { Title = "ValidTask", Duration = TimeSpan.FromDays(1) },
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByActualStartTimeItems()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", ActualStartTime = new DateTime(2021, 2, 2) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualStartTime = new DateTime(2021, 3, 3) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualStartTime = new DateTime(2021, 4, 4) },
                new ToDoItemDbEntry() { Title = "InvalidTask", ActualStartTime = new DateTime(2021, 5, 5) },
                new ToDoItemDbEntry() { Title = "InvalidTask", ActualStartTime = new DateTime(2021, 6, 6) },
                new ToDoItemDbEntry() { Title = "InvalidTask", ActualStartTime = new DateTime(2021, 7, 7) },
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByActualStartTime(new DateTime(2021, 1, 1), new DateTime(2021, 4, 4))
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", ActualStartTime = new DateTime(2021, 2, 2) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualStartTime = new DateTime(2021, 3, 3) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualStartTime = new DateTime(2021, 4, 4) },
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByActualEndTimeItems()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", ActualEndTime = new DateTime(2021, 2, 2) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualEndTime = new DateTime(2021, 3, 3) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualEndTime = new DateTime(2021, 4, 4) },
                new ToDoItemDbEntry() { Title = "InvalidTask", ActualEndTime = new DateTime(2021, 5, 5) },
                new ToDoItemDbEntry() { Title = "InvalidTask", ActualEndTime = new DateTime(2021, 6, 6) },
                new ToDoItemDbEntry() { Title = "InvalidTask", ActualEndTime = new DateTime(2021, 7, 7) },
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByActualEndTime(new DateTime(2021, 1, 1), new DateTime(2021, 4, 4))
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", ActualEndTime = new DateTime(2021, 2, 2) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualEndTime = new DateTime(2021, 3, 3) },
                new ToDoItemDbEntry() { Title = "ValidTask", ActualEndTime = new DateTime(2021, 4, 4) },
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByStatuses()
        {
            // Assert
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", ToDoItemStatusId = ToDoItemStatusIds.InProgress },
                new ToDoItemDbEntry() { Title = "ValidTask", ToDoItemStatusId = ToDoItemStatusIds.Paused },
                new ToDoItemDbEntry() { Title = "ValidTask", ToDoItemStatusId = ToDoItemStatusIds.Completed },
                new ToDoItemDbEntry() { Title = "InvalidTask", ToDoItemStatusId = ToDoItemStatusIds.Cancelled },
                new ToDoItemDbEntry() { Title = "InvalidTask", ToDoItemStatusId = ToDoItemStatusIds.Planned },
                new ToDoItemDbEntry() { Title = "InvalidTask", ToDoItemStatusId = ToDoItemStatusIds.Cancelled },
            };

            var targetStatuses = new List<int>() {
                    ToDoItemStatusIds.InProgress,
                    ToDoItemStatusIds.Paused,
                    ToDoItemStatusIds.Completed };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByStatuses(targetStatuses)
                .Build()
                .ToList();

            // Assert
            var expectedResult = new List<ToDoItemDbEntry>()
            {
                new ToDoItemDbEntry() { Title = "ValidTask", ToDoItemStatusId = ToDoItemStatusIds.InProgress },
                new ToDoItemDbEntry() { Title = "ValidTask", ToDoItemStatusId = ToDoItemStatusIds.Paused },
                new ToDoItemDbEntry() { Title = "ValidTask", ToDoItemStatusId = ToDoItemStatusIds.Completed },
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByCustomField_FromAndTo()
        {
            // Assert
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 18 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 20 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 23 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 15 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 70 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", StringValue = "string" } } }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByCustomFields(new List<CustomFieldsSearchOptionsBase> {
                    new CustomFieldsSearchOptions<int?> { Name = "Age", From = 18, To = 23 }
                })
                .Build();

            // Assert
            var expectedResult = new List<ToDoItemDbEntry>()
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 18 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 20 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 23 } } }
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByCustomField_OnlyFrom()
        {
            // Assert
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 18 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 20 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 23 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 70 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 15 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", StringValue = "string" } } }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByCustomFields(new List<CustomFieldsSearchOptionsBase> {
                    new CustomFieldsSearchOptions<int?> { Name = "Age", From = 18}
                })
                .Build();

            // Assert
            var expectedResult = new List<ToDoItemDbEntry>()
            {
                 new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 18 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 20 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 23 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 70 } } }
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByCustomField_OnlyTo()
        {
            // Assert
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 18 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 20 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 15 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 23 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 70 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", StringValue = "string" } } }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByCustomFields(new List<CustomFieldsSearchOptionsBase> {
                    new CustomFieldsSearchOptions<int?> { Name = "Age", To=20 }
                })
                .Build();

            // Assert
            var expectedResult = new List<ToDoItemDbEntry>()
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 18 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 20 } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 15 } } },
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByCustomField_Text()
        {
            // Assert
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Name", StringValue = "String" } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Name", StringValue = "Str" } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Name", StringValue = "Strin" } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 15 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 70 } } },
                new ToDoItemDbEntry() { Title = "InvalidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Age", IntValue = 30 } } }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByCustomFields(new List<CustomFieldsSearchOptionsBase> {
                    new CustomFieldsSearchOptions<string> { Name = "Name", Text = "Str" }
                })
                .Build();

            // Assert
            var expectedResult = new List<ToDoItemDbEntry>()
            {
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Name", StringValue = "String" } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Name", StringValue = "Str" } } },
                new ToDoItemDbEntry() { Title = "ValidTask", CustomFields = new List<CustomFieldDbEntry> { new CustomFieldDbEntry { Name = "Name", StringValue = "Strin" } } },
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByTags()
        {
            // Assert
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new ToDoItemDbEntry() {
                    Title = "ValidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } }
                    }
                },
                new ToDoItemDbEntry() {
                    Title = "ValidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag2" } },
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } }
                    }
                },
                new ToDoItemDbEntry() {
                    Title = "ValidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } }
                    }
                },

                new ToDoItemDbEntry() {
                    Title = "InvalidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag1" } }
                    }
                },
                new ToDoItemDbEntry() {
                    Title = "InvalidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag2" } }
                    }
                },
                new ToDoItemDbEntry() {
                    Title = "InvalidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } },
                    }
                }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByTags(new List<List<string>> { new List<string> { "Tag1", "Tag2" }, new List<string> { "Tag3" } })
                .Build();

            // Assert
            var expectedResult = new List<ToDoItemDbEntry>()
            {
                new ToDoItemDbEntry() {
                    Title = "ValidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } }
                    }
                },
                new ToDoItemDbEntry() {
                    Title = "ValidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag2" } },
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } }
                    }
                },
                new ToDoItemDbEntry() {
                    Title = "ValidTask",
                    ToDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>
                    {
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag1" } },
                        new ToDoItemTagAssociationDbEntry() { Tag = new TagDbEntry() { Value = "Tag3" } }
                    }
                }
            };

            returnedTasks.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByDeadLine()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2020, 12, 31)
                },
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2020, 01, 12)
                },
                new()
                {
                    Title = "Invalid",
                    Deadline = new DateTime(2222, 8, 16)
                }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByDeadLine(new DateTime(2020, 01, 12), new DateTime(2020, 12, 31))
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2020, 12, 31)
                },
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2020, 01, 12)
                }
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByDeadLine_When_DeadLineFromIsNull()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2222, 8, 16)
                }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByDeadLine(null, new DateTime(2223, 12, 31))
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2222, 8, 16)
                }
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByDeadLine_When_DeadLineToIsNull()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2222, 8, 16)
                }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByDeadLine(new DateTime(2021, 12, 31), null)
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2222, 8, 16)
                }
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsAllTasks_When_DeadLineFromIsNull_And_DeadLineToIsNull()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2222, 8, 16)
                },
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2333, 10, 12)
                }
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByDeadLine(null, null)
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>()
            {
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2222, 8, 16)
                },
                new()
                {
                    Title = "Valid",
                    Deadline = new DateTime(2333, 10, 12)
                }
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }

        [Fact]
        public void FilterItems_ReturnsFilteredByPlannedStartTimeItems()
        {
            // Arrange
            var toDoItems = new List<ToDoItemDbEntry>
            {
                new() { Title = "ValidTask", PlannedStartTime = new DateTime(2021, 2, 2) },
                new() { Title = "ValidTask", PlannedStartTime = new DateTime(2021, 3, 3) },
                new() { Title = "ValidTask", PlannedStartTime = new DateTime(2021, 4, 4) },
                new() { Title = "InvalidTask", PlannedStartTime = new DateTime(2021, 5, 5) },
                new() { Title = "InvalidTask", PlannedStartTime = new DateTime(2021, 6, 6) },
                new() { Title = "InvalidTask", PlannedStartTime = new DateTime(2021, 7, 7) },
            };

            var queryBuilder = new ToDoItemQueryBuilder();

            // Act
            var returnedTasks = queryBuilder
                .SetBaseQuery(toDoItems.AsQueryable())
                .ByPlannedStartTime(new DateTime(2021, 1, 1), new DateTime(2021, 4, 4))
                .Build()
                .ToList();

            // Assert
            var expectedTasks = new List<ToDoItemDbEntry>
            {
                new() { Title = "ValidTask", PlannedStartTime = new DateTime(2021, 2, 2) },
                new() { Title = "ValidTask", PlannedStartTime = new DateTime(2021, 3, 3) },
                new() { Title = "ValidTask", PlannedStartTime = new DateTime(2021, 4, 4) },
            };

            returnedTasks.Should().BeEquivalentTo(expectedTasks);
        }
    }
}