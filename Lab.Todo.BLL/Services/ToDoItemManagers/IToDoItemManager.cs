using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;

namespace Lab.Todo.BLL.Services.ToDoItemManagers
{
    public interface IToDoItemManager
    {
        Task<IEnumerable<ToDoItem>> GetAllTasksAsync(SearchOptions searchOptions = null);
        Task<ToDoItem> CreateNewTaskAsync(ToDoItemCreateData task);
        Task<ToDoItem> UpdateExistingTaskAsync(int taskItemId, ToDoItemUpdateData updatedTaskData);
        Task<IEnumerable<ToDoItem>> AddTagsAsync(IEnumerable<int> taskItemsIds, IEnumerable<string> tagsToAdd);
        Task DeleteTaskAsync(int taskItemId);
        Task<IEnumerable<ToDoItem>> RemoveTagsAsync(IEnumerable<int> taskItemsIds, IEnumerable<string> tagsToRemove);
        Task<ToDoItem> GetTaskByIdAsync(int taskItemId, bool throwExceptionIfTaskNotFound = true);
        Task<IEnumerable<Tag>> GetTagSuggestionsAsync(TimeSpan? usageTimeSpan = null);
        Task<IEnumerable<ToDoItemStatus>> GetAllStatusesAsync();
    }
}
