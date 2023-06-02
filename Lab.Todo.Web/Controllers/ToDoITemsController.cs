using AutoMapper;
using Lab.Todo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Exceptions;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;

namespace Lab.Todo.Web.Controllers
{
    public class ToDoItemsController : Controller
    {
        private readonly IToDoItemManager _toDoItemManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ToDoItemsController> _logger;

        public ToDoItemsController(IToDoItemManager toDoItemManager, IMapper mapper, ILogger<ToDoItemsController> logger)
        {
            _toDoItemManager = toDoItemManager;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ViewToDoItems() => await GetToDoItems();

        [HttpPost]
        public async Task<IActionResult> ViewToDoItems(ToDoItemSearchModel toDoItemSearchModel) => await GetToDoItems(toDoItemSearchModel);

        [HttpGet]
        public async Task<IActionResult> ViewToDoItemStatuses()
        {
            var allStatuses = await _toDoItemManager.GetAllStatusesAsync();
            var statuses = _mapper.Map<IEnumerable<ToDoItemStatus>, List<ToDoItemStatusModel>>(allStatuses);
            return View(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var toDoItemCreateModel = new ToDoItemCreateModel();

            await PopulateTagSuggestionsAsync(toDoItemCreateModel.TagViewModel);
            await PopulateToDoItemAddDependencyViewModel(toDoItemCreateModel.DependencyViewModel);

            return View(toDoItemCreateModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ToDoItemCreateModel itemCreateModel)
        {
            var toDoItemCreateData = _mapper.Map<ToDoItemCreateData>(itemCreateModel);

            await _toDoItemManager.CreateNewTaskAsync(toDoItemCreateData);
            _logger.LogDebug(_toDoItemManager.GetType().Name + " has successfully created new task.");

            return RedirectToAction(nameof(ViewToDoItems));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var toDoItem = await _toDoItemManager.GetTaskByIdAsync(id);
            var toDoItemUpdateModel = _mapper.Map<ToDoItemUpdateModel>(toDoItem);

            await PopulateTagSuggestionsAsync(toDoItemUpdateModel.TagViewModel);
            await PopulateToDoItemAddDependencyViewModel(toDoItemUpdateModel.DependencyViewModel, id);

            return View(toDoItemUpdateModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ToDoItemUpdateModel itemUpdateModel)
        {
            var itemUpdateData = _mapper.Map<ToDoItemUpdateData>(itemUpdateModel);

            try
            {
                await _toDoItemManager.UpdateExistingTaskAsync(itemUpdateModel.Id, itemUpdateData);
                _logger.LogDebug(_toDoItemManager.GetType().Name + " has successfully updated an existing task.");
            }
            catch (CyclicDependencyException ex)
            {
                await PopulateTagSuggestionsAsync(itemUpdateModel.TagViewModel);
                await PopulateToDoItemAddDependencyViewModel(itemUpdateModel.DependencyViewModel, itemUpdateModel.Id, ex.CyclicDependencies);
                itemUpdateModel.StatusModel.StatusSuggestions = Enum.GetNames(typeof(ToDoItemStatus)).ToList();

                return View(itemUpdateModel);
            }

            return RedirectToAction(nameof(ViewToDoItems));
        }

        [HttpPost]
        public async Task<IActionResult> AddTags(TagsAdditionModel additionDetails)
        {
            await _toDoItemManager.AddTagsAsync(additionDetails.ToDoItemIds, additionDetails.Tags);
            _logger.LogDebug(_toDoItemManager.GetType().Name + " has successfully added all tags.");

            return RedirectToAction(nameof(ViewToDoItems));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _toDoItemManager.DeleteTaskAsync(id);
            _logger.LogDebug(_toDoItemManager.GetType().Name + " has successfully deleted a task.");

            return RedirectToAction(nameof(ViewToDoItems));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTags(TagsRemovalModel removalDetails)
        {
            await _toDoItemManager.RemoveTagsAsync(removalDetails.ToDoItemIds, removalDetails.Tags);
            _logger.LogDebug(_toDoItemManager.GetType().Name + " has successfully removed all tags.");

            return RedirectToAction(nameof(ViewToDoItems));
        }

        private async Task PopulateTagSuggestionsAsync(TagViewModel tagViewModel)
        {
            var tagSuggestions = (await _toDoItemManager.GetTagSuggestionsAsync()).ToArray();
            var predefinedTagSuggestions = tagSuggestions.Where(tagSuggestion => tagSuggestion.IsPredefined);
            var notPredefinedTagSuggestions = tagSuggestions.Where(tagSuggestion => tagSuggestion.IsPredefined == false);

            tagViewModel.PredefinedTagSuggestions = _mapper.Map<IEnumerable<TagModel>>(predefinedTagSuggestions);
            tagViewModel.NotPredefinedTagSuggestions = _mapper.Map<IEnumerable<TagModel>>(notPredefinedTagSuggestions);
        }

        private async Task PopulateToDoItemAddDependencyViewModel(ToDoItemAddDependencyViewModel dependencyViewModel,
            int? taskId = null, IEnumerable<(int SourceId, IEnumerable<int> IdsCycle)> cyclicDependencies = null)
        {
            var toDoItems = await _toDoItemManager.GetAllTasksAsync();
            var availableDependencies = (_mapper.Map<IEnumerable<DependencyModel>>(toDoItems)).ToArray();

            dependencyViewModel.AvailableDependencies = (taskId is null) ? availableDependencies :
                availableDependencies.Where(dependency => dependency.ToDoItemId != taskId);
            dependencyViewModel.DependsOnItems ??= new List<int>();
            dependencyViewModel.HasAvailableDependencies = dependencyViewModel.AvailableDependencies.Any();
            dependencyViewModel.CyclicDependencies = cyclicDependencies?.Select(cyclicDependency =>
            {
                var source = availableDependencies.First(dependency => dependency.ToDoItemId == cyclicDependency.SourceId);
                var cycle = cyclicDependency.IdsCycle
                    .Select(dependencyId => availableDependencies.First(dependency => dependency.ToDoItemId == dependencyId));

                return (source, cycle);
            });
        }

        private async Task<IActionResult> GetToDoItems(ToDoItemSearchModel toDoItemSearchModel = null)
        {
            var toDoItems = await _toDoItemManager.GetAllTasksAsync(_mapper.Map<SearchOptions>(toDoItemSearchModel));

            var itemsViewModel = _mapper.Map<IEnumerable<ToDoItemViewModel>>(toDoItems);

            var taskListViewModel = new TaskListViewModel
            {
                AllTasks = itemsViewModel,
                AllStatuses = await _toDoItemManager.GetAllStatusesAsync()
            };

            return View(taskListViewModel);
        }
    }
}