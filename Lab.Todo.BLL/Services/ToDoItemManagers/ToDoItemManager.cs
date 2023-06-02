using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Builders;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Extensions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.BLL.Services.ToDoItemManagers.Options;
using Lab.Todo.BLL.Services.UserServices;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lab.Todo.BLL.Services.ToDoItemManagers
{
    public class ToDoItemManager : IToDoItemManager
    {
        private readonly ToDoItemManagerOptions _toDoItemManagerOptions;
        private readonly IMapper _mapper;
        private readonly ILogger<ToDoItemManager> _logger;
        private readonly IUnitOfWork _toDoUnitOfWork;
        private readonly IUserService _userService;

        private IToDoItemQueryBuilder _queryBuilder;

        public ToDoItemManager(IUnitOfWork unitOfWork, IUserService userService, IToDoItemQueryBuilder queryBuilder, IMapper mapper, IOptions<ToDoItemManagerOptions> options, ILogger<ToDoItemManager> logger)
        {
            _toDoUnitOfWork = unitOfWork;
            _userService = userService;
            _queryBuilder = queryBuilder;
            _mapper = mapper;
            _logger = logger;
            _toDoItemManagerOptions = options.Value;
        }

        public async Task<ToDoItem> CreateNewTaskAsync(ToDoItemCreateData createdTaskData)
        {
            if (createdTaskData.DependsOnItems is not null)
            {
                createdTaskData.DependsOnItems = createdTaskData.DependsOnItems
                    .Select(x => x)
                    .DistinctBy(x => x.ToDoItemId)
                    .ToList();

                ThrowExceptionIfDependencyAmountExceedsLimit(createdTaskData.DependsOnItems.Count());
                await ThrowExceptionIfUnableToCreateDependencyAsync(createdTaskData.DependsOnItems);
            }

            createdTaskData.AssignedTo ??= _userService.Current.Email;

            var toDoItemDbEntry = _mapper.Map<ToDoItemDbEntry>(createdTaskData);

            if (createdTaskData.Tags is not null && createdTaskData.Tags.Any())
            {
                await AddTagsAsync(toDoItemDbEntry, createdTaskData.Tags);
            }

            if (createdTaskData.ParentTaskId is not null)
            {
                var parentTask = await _toDoUnitOfWork.ToDoItems.GetByIdAsync((int)createdTaskData.ParentTaskId, true);

                if (parentTask is null)
                {
                    throw new NotFoundException($"Parent task with Id {createdTaskData.ParentTaskId} does not exist.");
                }

                if (parentTask.ParentTaskId is not null)
                {
                    await ThrowExceptionIfThereAreCyclicParentTasksAsync(parentTask.Id, (int)parentTask.ParentTaskId);
                }

                toDoItemDbEntry.ParentTask = parentTask;
            }

            toDoItemDbEntry.CreatedBy = _userService.Current.Email;
            toDoItemDbEntry.CreatedDate = DateTime.UtcNow;

            _toDoUnitOfWork.ToDoItems.Create(toDoItemDbEntry);
            _logger.LogDebug(toDoItemDbEntry.GetType().Name + " has successfully created new task.");

            await _toDoUnitOfWork.SaveChangesAsync();

            var createdTask = _mapper.Map<ToDoItem>(toDoItemDbEntry);

            await PopulateDependencyCollectionWithTitlesAsync(createdTask.DependsOnItems);
            await PopulateDependencyCollectionWithTitlesAsync(createdTask.DependentItems);

            return createdTask;
        }

        public async Task<ToDoItem> UpdateExistingTaskAsync(int taskItemId, ToDoItemUpdateData updatedTaskData)
        {
            var returnedTask = await _toDoUnitOfWork.ToDoItems.GetByIdAsync(taskItemId, true);
            if (returnedTask is null)
            {
                throw new NotFoundException($"Task with Id {taskItemId} does not exist.");
            }

            ValidateUserPermissions(returnedTask);

            if (updatedTaskData.AssignedTo != returnedTask.AssignedTo
                && _userService.Current.Email != returnedTask.CreatedBy)
            {
                throw new PermissionsException("Only creator of the task is able to set AssignedTo field");
            }

            if (updatedTaskData.ParentTaskId is not null)
            {
                var parentTask = await _toDoUnitOfWork.ToDoItems.GetByIdAsync((int)updatedTaskData.ParentTaskId, true);

                if (parentTask is null)
                {
                    throw new NotFoundException($"Parent task with Id {updatedTaskData.ParentTaskId} does not exist.");
                }

                if (parentTask.ParentTaskId is not null)
                {
                    await ThrowExceptionIfThereAreCyclicParentTasksAsync(parentTask.Id, (int)parentTask.ParentTaskId);
                }

                await ThrowExceptionIfThereAreCyclicParentTasksAsync(taskItemId, (int)updatedTaskData.ParentTaskId);

                returnedTask.ParentTask = parentTask;
            }

            SetStartTimeEndTime(returnedTask, updatedTaskData);

            if (updatedTaskData.DependsOnItems?.Any() is true)
            {
                updatedTaskData.DependsOnItems = updatedTaskData.DependsOnItems
                    .Where(dep => dep.ToDoItemId != taskItemId)
                    .Select(x => x)
                    .DistinctBy(x => x.ToDoItemId)
                    .ToList();

                var dependenciesIds = updatedTaskData.DependsOnItems.Select(dependency => dependency.ToDoItemId);
                await ThrowExceptionIfThereAreCyclicDependenciesAsync(taskItemId, dependenciesIds);

                ThrowExceptionIfDependencyAmountExceedsLimit(updatedTaskData.DependsOnItems.Count());
                await ThrowExceptionIfUnableToCreateDependencyAsync(updatedTaskData.DependsOnItems);
            }

            _mapper.Map(updatedTaskData, returnedTask);

            await UpdateTagsAsync(returnedTask, updatedTaskData.Tags);

            returnedTask.ModifiedBy = _userService.Current.Email;
            returnedTask.ModifiedDate = DateTime.UtcNow;
            _toDoUnitOfWork.ToDoItems.Update(returnedTask);
            _logger.LogDebug(returnedTask.GetType().Name + " has successfully updated existing task.");

            await _toDoUnitOfWork.SaveChangesAsync();

            var updatedTask = _mapper.Map<ToDoItem>(returnedTask);

            await PopulateDependencyCollectionWithTitlesAsync(updatedTask.DependsOnItems);
            await PopulateDependencyCollectionWithTitlesAsync(updatedTask.DependentItems);

            return updatedTask;
        }

        private static void SetStartTimeEndTime(ToDoItemDbEntry toDoItem, ToDoItemUpdateData toDoItemUpdateData)
        {
            var newStatus = toDoItemUpdateData.Status;
            switch (newStatus)
            {
                case ToDoItemStatus.InProgress:
                    {
                        toDoItem.ActualStartTime = DateTime.UtcNow;
                        toDoItem.ActualEndTime = null;
                        break;
                    }
                case ToDoItemStatus.Completed:
                    {
                        toDoItem.ActualStartTime ??= DateTime.UtcNow;
                        toDoItem.ActualEndTime = DateTime.UtcNow;
                        break;
                    }
                case ToDoItemStatus.Planned:
                    {
                        toDoItem.ActualStartTime = null;
                        toDoItem.ActualEndTime = null;
                        break;
                    }
            }
        }

        public async Task<IEnumerable<ToDoItem>> AddTagsAsync(IEnumerable<int> taskItemsIds, IEnumerable<string> tagsToAdd)
        {
            var requestedIds = taskItemsIds as int[] ?? taskItemsIds.ToArray();
            var returnedTasks = (await _toDoUnitOfWork.ToDoItems.GetByIdAsync(requestedIds, true)).ToArray();
            ThrowNotFoundExceptionIfToDoItemsDoNotExist(requestedIds, returnedTasks);

            ValidateUserPermissions(returnedTasks.ToArray());

            var toAdd = tagsToAdd as string[] ?? tagsToAdd.ToArray();
            var tagsToAddTitleCase = toAdd.Select(tag => tag.ToTitleCase()).ToList();
            var tagsToAddDbEntries = (await _toDoUnitOfWork.Tags.GetByValuesAsync(toAdd, true)).ToList();
            PopulateTagsToAddDbEntries(tagsToAddDbEntries, tagsToAddTitleCase);

            foreach (var returnedTask in returnedTasks)
            {
                var tags = returnedTask.ToDoItemTagAssociations
                    .Select(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value)
                    .Union(tagsToAddTitleCase)
                    .ToList();

                ThrowExceptionIfTagAmountExceedsLimit(tags.Count);

                ModifyToDoItemTagAssociations(returnedTask, tags, tagsToAddDbEntries);
            }

            _logger.LogDebug(returnedTasks.GetType().Name + " has successfully added new tags associations.");
            await _toDoUnitOfWork.SaveChangesAsync();

            var updatedTasks = _mapper.Map<IEnumerable<ToDoItem>>(returnedTasks);

            return updatedTasks;
        }

        public async Task DeleteTaskAsync(int taskItemId)
        {
            var taskToDelete = await _toDoUnitOfWork.ToDoItems.GetByIdAsync(taskItemId);
            if (taskToDelete is null)
            {
                throw new NotFoundException($"Task with Id {taskItemId} does not exist.");
            }

            ValidateUserPermissions(taskToDelete);

            if (await _toDoUnitOfWork.ToDoItems.HasChildTasksAsync(taskItemId))
            {
                throw new ApplicationException($"Unable to delete. The task with Id {taskItemId} has child tasks.");
            }

            foreach (var toDoItemTagAssociation in taskToDelete.ToDoItemTagAssociations)
            {
                _toDoUnitOfWork.ToDoItemTagAssociations.Delete(toDoItemTagAssociation);
            }

            if (taskToDelete.DependsOnItems is not null)
            {
                foreach (var dependency in taskToDelete.DependsOnItems)
                {
                    _toDoUnitOfWork.ToDoItemDependencies.Delete(dependency);
                }
            }

            if (taskToDelete.DependentItems is not null)
            {
                foreach (var dependency in taskToDelete.DependentItems)
                {
                    _toDoUnitOfWork.ToDoItemDependencies.Delete(dependency);
                }
            }

            if (taskToDelete.CustomFields is not null)
            {
                _toDoUnitOfWork.CustomFields.DeleteRelatedToToDoItem(taskItemId);
            }

            _toDoUnitOfWork.ToDoItems.Delete(taskItemId);
            _logger.LogDebug(taskToDelete.GetType().Name + " has successfully deleted the task.");
            await _toDoUnitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ToDoItem>> RemoveTagsAsync(IEnumerable<int> taskItemsIds, IEnumerable<string> tagsToRemove)
        {
            var requestedIds = taskItemsIds as int[] ?? taskItemsIds.ToArray();
            var returnedTasks = await _toDoUnitOfWork.ToDoItems.GetByIdAsync(requestedIds, true);
            var toDoItemDbEntries = returnedTasks as ToDoItemDbEntry[] ?? returnedTasks.ToArray();
            ThrowNotFoundExceptionIfToDoItemsDoNotExist(requestedIds, toDoItemDbEntries);
            ValidateUserPermissions(toDoItemDbEntries.ToArray());

            var tagsToRemoveTitleCase = tagsToRemove.Select(tag => tag.ToTitleCase()).ToList();

            foreach (var returnedTask in toDoItemDbEntries)
            {
                var tags = returnedTask.ToDoItemTagAssociations
                    .Select(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value)
                    .Except(tagsToRemoveTitleCase)
                    .ToList();

                ThrowExceptionIfThereAreExtraTags(returnedTask, tags, tagsToRemoveTitleCase);

                ModifyToDoItemTagAssociations(returnedTask, tags, tagsToRemove: tagsToRemoveTitleCase);
            }

            _logger.LogDebug(returnedTasks.GetType().Name + " has successfully deleted tags associations.");
            await _toDoUnitOfWork.SaveChangesAsync();

            var updatedTasks = _mapper.Map<IEnumerable<ToDoItem>>(returnedTasks);

            return updatedTasks;
        }

        public async Task<ToDoItem> GetTaskByIdAsync(int taskItemId, bool throwExceptionIfTaskNotFound = true)
        {
            var returnedTask = await _toDoUnitOfWork.ToDoItems.GetByIdAsync(taskItemId);
            if (returnedTask is null)
            {
                if (throwExceptionIfTaskNotFound)
                {
                    throw new NotFoundException($"Task with Id {taskItemId} does not exist.");
                }

                return null;
            }

            ValidateUserPermissions(returnedTask);

            var task = _mapper.Map<ToDoItem>(returnedTask);

            await PopulateDependencyCollectionWithTitlesAsync(task.DependsOnItems);
            await PopulateDependencyCollectionWithTitlesAsync(task.DependentItems);

            return task;
        }

        public async Task<IEnumerable<ToDoItem>> GetAllTasksAsync(SearchOptions searchOptions = null)
        {
            var query = _toDoUnitOfWork.ToDoItems.GetAllAsQueryable();
            _queryBuilder = _queryBuilder.SetBaseQuery(query);

            if (searchOptions != null)
            {
                _queryBuilder = _queryBuilder
                    .ByTitle(searchOptions.Title)
                    .ByDescription(searchOptions.Description)
                    .ByParentId(searchOptions.ParentTaskId)
                    .ByStatuses(_mapper.Map<IEnumerable<int>>(searchOptions.Statuses))
                    .ByDuration(searchOptions.DurationFrom, searchOptions.DurationTo)
                    .ByActualStartTime(searchOptions.ActualStartTimeFrom, searchOptions.ActualStartTimeTo)
                    .ByActualEndTime(searchOptions.ActualEndTimeFrom, searchOptions.ActualEndTimeTo)
                    .ByTags(searchOptions.Tags)
                    .ByCustomFields(searchOptions.CustomFields)
                    .ByDeadLine(searchOptions.DeadlineFrom, searchOptions.DeadlineTo)
                    .ByPlannedStartTime(searchOptions.PlannedStartTimeFrom, searchOptions.PlannedStartTimeTo);
            }

            query = _queryBuilder
                .ByCreatorOrPerformer(_userService.Current.Email)
                .Build();

            var returnedTasks = (query is IAsyncEnumerable<ToDoItemDbEntry>) ? await query.ToListAsync() : query.ToList();

            var toDoItems = _mapper.Map<IEnumerable<ToDoItem>>(returnedTasks).ToArray();
            foreach (var toDoItem in toDoItems)
            {
                await PopulateDependencyCollectionWithTitlesAsync(toDoItem.DependsOnItems);
                await PopulateDependencyCollectionWithTitlesAsync(toDoItem.DependentItems);
            }

            return toDoItems;
        }


        public async Task<IEnumerable<Tag>> GetTagSuggestionsAsync(TimeSpan? usageTimeSpan = null)
        {
            var userEmail = _userService.Current.Email;
            var usageDateLowerBound = (usageTimeSpan is null) ? null : (DateTime?)DateTime.UtcNow.Subtract(usageTimeSpan.Value);

            var mostUsedTags = await _toDoUnitOfWork.ToDoItemTagAssociations.GetMostUsedTagsAsync(userEmail, usageDateLowerBound, true);
            var predefinedTags = await _toDoUnitOfWork.Tags.GetPredefinedAsync(true);
            var tagSuggestionDbEntry = mostUsedTags.Union(predefinedTags);
            var tagSuggestions = _mapper.Map<IEnumerable<Tag>>(tagSuggestionDbEntry);

            return tagSuggestions;
        }

        public async Task<IEnumerable<ToDoItemStatus>> GetAllStatusesAsync()
        {
            var statuses = await _toDoUnitOfWork.ToDoItemStatuses.GetAllAsync();
            var mappedStatuses = _mapper.Map<IEnumerable<ToDoItemStatus>>(statuses);

            return mappedStatuses;
        }

        private async Task AddTagsAsync(ToDoItemDbEntry toDoItemDbEntry, IEnumerable<string> tags)
        {
            toDoItemDbEntry.ToDoItemTagAssociations = new();

            await UpdateTagsAsync(toDoItemDbEntry, tags);
        }

        private async Task UpdateTagsAsync(ToDoItemDbEntry toDoItemDbEntry, IEnumerable<string> tags)
        {
            var tagsArray = tags?.ToArray();
            if (tagsArray is not null)
            {
                ThrowExceptionIfTagAmountExceedsLimit(tagsArray.Length);
            }

            var tagsTitleCase = tagsArray?.Select(tag => tag.ToTitleCase()).ToList() ?? new();
            ThrowExceptionIfTagsHaveDuplicates(tagsTitleCase);

            var existingTags = toDoItemDbEntry.ToDoItemTagAssociations?.Select(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value).ToList() ?? new List<string>();
            var tagsToRemove = existingTags.Except(tagsTitleCase);
            var tagsToAdd = tagsTitleCase.Except(existingTags).ToArray();
            var tagsToAddDbEntries = tagsToAdd.Any() ? (await _toDoUnitOfWork.Tags.GetByValuesAsync(tagsToAdd, true)).ToList() : new();
            PopulateTagsToAddDbEntries(tagsToAddDbEntries, tagsToAdd);

            ModifyToDoItemTagAssociations(toDoItemDbEntry, tagsTitleCase, tagsToAddDbEntries, tagsToRemove);
        }

        private void ModifyToDoItemTagAssociations(ToDoItemDbEntry toDoItemDbEntry, List<string> tags,
            IEnumerable<TagDbEntry> tagsToAddDbEntries = null, IEnumerable<string> tagsToRemove = null)
        {
            tagsToRemove ??= new List<string>();
            var toDoItemTagAssociations = new List<ToDoItemTagAssociationDbEntry>();
            var toAddDbEntries = tagsToAddDbEntries?.ToArray();
            for (var index = 0; index < tags.Count; index++)
            {
                CreateOrUpdateToDoItemTagAssociation(toDoItemDbEntry, toDoItemTagAssociations, toAddDbEntries,
                    tags[index], index);
            }

            foreach (var tagToRemove in tagsToRemove)
            {
                var tagToRemoveDbEntry = toDoItemDbEntry.ToDoItemTagAssociations
                    .Find(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value == tagToRemove);

                _toDoUnitOfWork.ToDoItemTagAssociations.Delete(tagToRemoveDbEntry);
            }

            toDoItemDbEntry.ToDoItemTagAssociations = toDoItemTagAssociations;
        }

        private void ThrowExceptionIfTagAmountExceedsLimit(int tagAmount)
        {
            if (tagAmount > _toDoItemManagerOptions.MaximumTagAmount)
            {
                throw new TagLimitException($"Task can't have more than {_toDoItemManagerOptions.MaximumTagAmount} tags.");
            }
        }

        private static void ThrowExceptionIfTagsHaveDuplicates(IEnumerable<string> tags)
        {
            var duplicateExists = tags.GroupBy(tag => tag).Any(group => group.Count() > 1);

            if (duplicateExists)
            {
                throw new TagDuplicateException("Tags have one or more duplicates.");
            }
        }

        private static void ThrowExceptionIfThereAreExtraTags(ToDoItemDbEntry toDoItemDbEntry, List<string> tagsAfterRemoval,
            List<string> tagsToRemove)
        {
            if ((tagsAfterRemoval.Count + tagsToRemove.Count) != toDoItemDbEntry.ToDoItemTagAssociations.Count)
            {
                var extraTags = tagsToRemove
                    .Except(toDoItemDbEntry.ToDoItemTagAssociations.Select(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value));
                var extraTagsWithSeparator = string.Join(", ", extraTags);

                throw new ExtraTagException($"Task with Id {toDoItemDbEntry.Id} doesn't have the following tags: {extraTagsWithSeparator}.");
            }
        }

        private static void ThrowNotFoundExceptionIfToDoItemsDoNotExist(IEnumerable<int> requestedIds, IEnumerable<ToDoItemDbEntry> returnedTasks)
        {
            var toDoItemDbEntries = returnedTasks as ToDoItemDbEntry[] ?? returnedTasks.ToArray();
            var enumerable = requestedIds as int[] ?? requestedIds.ToArray();
            if (toDoItemDbEntries.Length != enumerable.Length)
            {
                var returnedTasksIds = toDoItemDbEntries.Select(task => task.Id);
                var notExistingIds = enumerable.Except(returnedTasksIds).OrderBy(taskItemId => taskItemId).ToList();

                ThrowNotFoundException(notExistingIds);
            }
        }

        private static void ThrowNotFoundException(List<int> notExistingIds)
        {
            if (notExistingIds.Count == 1)
            {
                throw new NotFoundException($"Task with Id {notExistingIds[0]} does not exist.");
            }

            var idsAsString = string.Join(", ", notExistingIds);

            throw new NotFoundException($"Tasks with Ids {idsAsString} do not exist.");
        }

        private async Task ThrowExceptionIfThereAreCyclicDependenciesAsync(int toDoItemId, IEnumerable<int> dependenciesIds)
        {
            var cyclicDependencies = (await _toDoUnitOfWork.ToDoItemDependencies.GetCyclicDependenciesAsync(toDoItemId, dependenciesIds)).ToArray();

            if (cyclicDependencies.Any())
            {
                var cyclicDependenciesTuples = new List<(int CyclicDependencySourceId, IEnumerable<int> IdsCycle)>();
                var cycleMessages = new List<string>();

                foreach (var cyclicDependency in cyclicDependencies)
                {
                    cyclicDependenciesTuples.Add((cyclicDependency.IdsCycle.ElementAt(1), cyclicDependency.IdsCycle));
                    cycleMessages.Add($"Cycle source Id: {cyclicDependency.IdsCycle.ElementAt(1)}, cycle: {string.Join("->", cyclicDependency.IdsCycle)}.");
                }

                var exceptionMessage = $"Adding dependencies creates {(cyclicDependencies.Length == 1 ? "cycle" : "cycles")}:" +
                    Environment.NewLine + string.Join(Environment.NewLine, cycleMessages);

                throw new CyclicDependencyException(exceptionMessage) { CyclicDependencies = cyclicDependenciesTuples };
            }
        }

        private async Task ThrowExceptionIfThereAreCyclicParentTasksAsync(int toDoItemId, int parentTaskId)
        {
            var cyclicParentTasks = (await _toDoUnitOfWork.ToDoItemDependencies.GetCyclicParentTaskAsync(toDoItemId, parentTaskId)).ToArray();
            
            if (cyclicParentTasks.Any())
            {
                var cyclicParentTasksTuples = new List<(int cyclicParentTaskSourceId, IEnumerable<int> idsCycle)>();
                var cycleMessages = new List<string>();
                
                foreach (var cyclicParentTask in cyclicParentTasks)
                {
                    cyclicParentTasksTuples.Add((cyclicParentTask.IdsCycle.ElementAt(1), cyclicParentTask.IdsCycle));
                    cycleMessages.Add($"Cycle source Id: {cyclicParentTask.IdsCycle.ElementAt(1)}, cycle: {string.Join("->", cyclicParentTask.IdsCycle)}.");
                }
                
                var exceptionMessage =
                    $"Adding parent task creates {(cyclicParentTasks.Length == 1 ? "cycle" : "cycles")}:" +
                    $"{Environment.NewLine}{string.Join(Environment.NewLine, cycleMessages)}";
                
                throw new CyclicParentTaskException(exceptionMessage) { CyclicParentTasks = cyclicParentTasksTuples };
            }
        }

        private void CreateOrUpdateToDoItemTagAssociation(ToDoItemDbEntry toDoItem, List<ToDoItemTagAssociationDbEntry> toDoItemTagAssociations,
            IEnumerable<TagDbEntry> tagsToAddDbEntries, string tagValue, int order)
        {
            var toDoItemTagAssociation = toDoItem.ToDoItemTagAssociations
                    .FirstOrDefault(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value == tagValue);

            if (toDoItemTagAssociation is null)
            {
                var tagToAddDbEntry = tagsToAddDbEntries.First(tag => tag.Value == tagValue);
                toDoItemTagAssociation = new ToDoItemTagAssociationDbEntry { Tag = tagToAddDbEntry, ToDoItem = toDoItem, Order = order };

                _toDoUnitOfWork.ToDoItemTagAssociations.Create(toDoItemTagAssociation);
            }
            else if (toDoItemTagAssociation.Order != order)
            {
                toDoItemTagAssociation.Order = order;

                _toDoUnitOfWork.ToDoItemTagAssociations.Update(toDoItemTagAssociation);
            }

            toDoItemTagAssociations.Add(toDoItemTagAssociation);
        }

        private void PopulateTagsToAddDbEntries(List<TagDbEntry> tagsToAddDbEntries, IEnumerable<string> tagsToAdd)
        {
            foreach (var tagToAdd in tagsToAdd)
            {
                var tagToAddDbEntry = tagsToAddDbEntries.FirstOrDefault(tag => tag.Value == tagToAdd);

                if (tagToAddDbEntry is null)
                {
                    tagToAddDbEntry = new TagDbEntry { Value = tagToAdd };

                    tagsToAddDbEntries.Add(tagToAddDbEntry);
                    _toDoUnitOfWork.Tags.Create(tagToAddDbEntry);
                }
            }
        }

        private async Task PopulateDependencyCollectionWithTitlesAsync(IEnumerable<ToDoItemDependency> dependencies)
        {
            if (dependencies is null)
            {
                return;
            }

            List<int> dependenciesIds = new();
            var toDoItemDependencies = dependencies.ToArray();
            foreach (var dependency in toDoItemDependencies)
            {
                dependenciesIds.Add(dependency.ToDoItemId);
            }

            var dictionaryOfDependenciesData = await _toDoUnitOfWork.ToDoItems.GetTitlesByIdsAsync(dependenciesIds);

            if (dictionaryOfDependenciesData is null)
            {
                return;
            }

            foreach (var dependency in toDoItemDependencies)
            {
                dependency.ToDoItemTitle = dictionaryOfDependenciesData[dependency.ToDoItemId];
            }
        }

        private async Task ThrowExceptionIfUnableToCreateDependencyAsync(IEnumerable<ToDoItemDependencyCreateUpdateData> dependencies)
        {
            var toDoItemsIds = (await _toDoUnitOfWork.ToDoItems.GetAllIdsAsync()).ToArray();
            foreach (var dependency in dependencies)
            {
                if (!toDoItemsIds.Contains(dependency.ToDoItemId))
                {
                    throw new WrongDependencyException($"Wrong dependent item id, item with id = {dependency.ToDoItemId} doesn't exists.");
                }
            }
        }

        private void ThrowExceptionIfDependencyAmountExceedsLimit(int dependencyCount)
        {
            if (dependencyCount > _toDoItemManagerOptions.MaximumDependencyAmount)
            {
                throw new WrongDependencyException($"Task can't have more than {_toDoItemManagerOptions.MaximumDependencyAmount} dependencies.");
            }
        }

        private void ValidateUserPermissions(params ToDoItemDbEntry[] toDoItems)
        {
            var username = _userService.Current.Email;
            var toDoItemIds = toDoItems.Select(item => item.Id);
            var authorizedIds = toDoItems
                .Where(item => string.IsNullOrWhiteSpace(item.CreatedBy) || string.IsNullOrWhiteSpace(item.AssignedTo) ||
                    item.CreatedBy == username || item.AssignedTo == username)
                .Select(task => task.Id);

            var notAuthorizedIds = toDoItemIds.Except(authorizedIds).OrderBy(taskItemId => taskItemId).ToList();
            if (notAuthorizedIds.Count > 0)
            {
                ThrowUnauthorizedUserException(notAuthorizedIds);
            }
        }

        private static void ThrowUnauthorizedUserException(List<int> notAuthorizedIds)
        {
            if (notAuthorizedIds.Count == 1)
            {
                throw new UnauthorizedUserException($"Task with Id {notAuthorizedIds[0]} can't be accessed by current user.");
            }

            var idsAsString = string.Join(", ", notAuthorizedIds);

            throw new UnauthorizedUserException($"Tasks with Ids {idsAsString} can't be accessed by current user.");
        }
    }
}