using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Todo.Api.DTOs.Queries;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Refit;

namespace Lab.Todo.SmokeTests.Services.DataAccess
{
    [Headers("Authorization: " + JwtBearerDefaults.AuthenticationScheme)]
    public interface IToDoItemsService
    {
        [Post("/api/ToDoItems")]
        Task<ToDoItemCreateResponse?> AddToDoItem([Body] ToDoItemCreateRequest toDoItem);
        
        [Get("/api/ToDoItems/{id}")]
        Task<ToDoItemGetResponse?> GetToDoItemById(int id);

        [Get("/api/ToDoItems")]
        Task<IEnumerable<ToDoItemGetResponse>?> GetAllToDoItems();

        [Post("/api/ToDoItems/search")]
        Task<IEnumerable<ToDoItemGetResponse>?> GetAllToDoItems([Body] ToDoItemSearchQuery searchQuery);
        
        [Put("/api/ToDoItems/{id}")]
        Task<ToDoItemUpdateResponse?> UpdateToDoItem(int id, [Body] ToDoItemUpdateRequest itemUpdateRequest);
        
        [Delete("/api/ToDoItems/{id}")]
        Task DeleteToDoItem(int id);

        [Get("/api/ToDoItems")]
        Task<IEnumerable<TagGetResponse>?> GetTagSuggestions([AliasAs("tag-suggestions")] TagSuggestionsQuery query);
        
        [Put("/api/ToDoItems/tags")]
        Task<IEnumerable<ToDoItemUpdateResponse>?> AddTags([Body] TagsAdditionRequest tagsAdditionRequest);
        
        [Delete("/api/ToDoItems/tags")]
        Task<IEnumerable<ToDoItemUpdateResponse>?> RemoveTags([Body] TagsRemovalRequest tagsRemovalRequest);
        
        [Post("/api/Attachments")]
        Task<AttachmentCreateResponse?> CreateAttachment([Body] AttachmentCreateRequest attachment);
        
        [Get("/api/Attachments/{id}")]
        Task GetAttachment(int id);

        [Get("/api/ToDoItems/statuses")]
        Task<IEnumerable<ToDoItemStatusGetResponse>?> GetToDoItemStatuses();
    }
}