using AutoMapper;
using Lab.Todo.Api.DTOs.Queries;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Todo.BLL.Services.ToDoItemManagers;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;

namespace Lab.Todo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoItemsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IToDoItemManager _toDoItemManager;

        public ToDoItemsController(IMapper mapper, IToDoItemManager toDoItemManager)
        {
            _mapper = mapper;
            _toDoItemManager = toDoItemManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoItemGetResponse>>> GetAllToDoItems() => await GetToDoItems();
        
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<ToDoItemGetResponse>>> GetToDoItemsBySearchQuery(ToDoItemSearchQuery searchQuery) => await GetToDoItems(searchQuery);

        [HttpGet("statuses")]
        public async Task<ActionResult<IEnumerable<ToDoItemStatusGetResponse>>> GetToDoItemStatuses()
        {
            var statuses = await _toDoItemManager.GetAllStatusesAsync();

            var toDoItemStatusResponse = _mapper.Map<IEnumerable<ToDoItemStatusGetResponse>>(statuses);

            return Ok(toDoItemStatusResponse);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemGetResponse>> GetToDoItem(int id)
        {
            var toDoItem = await _toDoItemManager.GetTaskByIdAsync(id);
            var toDoItemResponse = _mapper.Map<ToDoItemGetResponse>(toDoItem);
            return Ok(toDoItemResponse);
        }

        [HttpGet("tag-suggestions")]
        public async Task<ActionResult<IEnumerable<TagGetResponse>>> GetTagSuggestions([FromQuery] TagSuggestionsQuery query)
        {
            var usageTimeSpan = (query.UsageTimeSpanInDays is null) ?
                null : (TimeSpan?)TimeSpan.FromDays(query.UsageTimeSpanInDays.Value);
            var tagSuggestions = await _toDoItemManager.GetTagSuggestionsAsync(usageTimeSpan);
            var tagSuggestionResponse = _mapper.Map<IEnumerable<TagGetResponse>>(tagSuggestions);

            return Ok(tagSuggestionResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToDoItem(ToDoItemCreateRequest itemCreateRequest)
        {
            var toDoItemCreateData = _mapper.Map<ToDoItemCreateData>(itemCreateRequest);
            var toDoItem = await _toDoItemManager.CreateNewTaskAsync(toDoItemCreateData);
            var toDoItemResponse = _mapper.Map<ToDoItemCreateResponse>(toDoItem);

            return CreatedAtAction(nameof(GetToDoItem), new { id = toDoItemResponse.Id }, toDoItemResponse);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ToDoItemUpdateResponse>> UpdateToDoItem(int id, ToDoItemUpdateRequest itemUpdateRequest)
        {
            var toDoItemUpdateData = _mapper.Map<ToDoItemUpdateData>(itemUpdateRequest);
            var updatedToDoItem = await _toDoItemManager.UpdateExistingTaskAsync(id, toDoItemUpdateData);
            var toDoItemResponse = _mapper.Map<ToDoItemUpdateResponse>(updatedToDoItem);

            return Ok(toDoItemResponse);
        }

        [HttpPut("tags")]
        public async Task<ActionResult<IEnumerable<ToDoItemUpdateResponse>>> AddTags(TagsAdditionRequest additionDetails)
        {
            var updatedToDoItems = await _toDoItemManager.AddTagsAsync(additionDetails.ToDoItemIds, additionDetails.Tags);
            var toDoItemsResponse = _mapper.Map<IEnumerable<ToDoItemUpdateResponse>>(updatedToDoItems);

            return Ok(toDoItemsResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDoItem(int id)
        {
            await _toDoItemManager.DeleteTaskAsync(id);

            return NoContent();
        }

        [HttpDelete("tags")]
        public async Task<ActionResult<IEnumerable<ToDoItemUpdateResponse>>> RemoveTags(TagsRemovalRequest removalDetails)
        {
            var updatedToDoItems = await _toDoItemManager.RemoveTagsAsync(removalDetails.ToDoItemIds, removalDetails.Tags);
            var toDoItemsResponse = _mapper.Map<IEnumerable<ToDoItemUpdateResponse>>(updatedToDoItems);

            return Ok(toDoItemsResponse);
        }

        private async Task<ActionResult<IEnumerable<ToDoItemGetResponse>>> GetToDoItems(ToDoItemSearchQuery toDoItemSearchQuery = null)
        {
            var toDoItems = await _toDoItemManager.GetAllTasksAsync(_mapper.Map<SearchOptions>(toDoItemSearchQuery));

            var toDoItemsResponse = _mapper.Map<IEnumerable<ToDoItemGetResponse>>(toDoItems);

            return Ok(toDoItemsResponse);
        }
    }
}