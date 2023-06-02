using FluentAssertions;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.SmokeTests.DatabaseModels;
using Lab.Todo.SmokeTests.Drivers;
using Lab.Todo.SmokeTests.Helpers;
using Lab.Todo.SmokeTests.Options;
using Lab.Todo.SmokeTests.Services.DataAccess;
using Lab.Todo.SmokeTests.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Lab.Todo.SmokeTests.Steps
{
    [Binding]
    public class GetToDoItemsSteps
    {
        private readonly IToDoItemsService _toDoItemsDataAccess;
        private readonly IToDoItemsDbAccessor _toDoItemsDbAccessor;
        private readonly UserOptions _userOptions;
        private readonly DatabaseCleanupDriver _databaseCleanupDriver;
        private readonly IAuthorizationManager _authorizationManager;
        
        private IEnumerable<ToDoItemGetResponse>? _responseToDoItems;

        public GetToDoItemsSteps(
            IToDoItemsService toDoItemsDataAccess,
            IToDoItemsDbAccessor toDoItemsDbAccessor,
            IOptions<UserOptions> userOptions,
            DatabaseCleanupDriver databaseCleanupDriver, 
            IAuthorizationManager authorizationManager)
        {   
            _toDoItemsDataAccess = toDoItemsDataAccess;
            _toDoItemsDbAccessor = toDoItemsDbAccessor;
            _userOptions = userOptions.Value;
            _databaseCleanupDriver = databaseCleanupDriver;
            _authorizationManager = authorizationManager;
        }

        [Given(@"the next tasks in database")]
        public async Task GivenTheNextTasksInDatabase(IEnumerable<ToDoItemDbModel> toDoItems)
        {
            foreach (var item in toDoItems)
            {
                if (await _toDoItemsDbAccessor.GetByFilterAsync<ToDoItemDbModel>("ToDoItem", item) is not null)
                {
                    continue;
                }

                item.CreatedBy ??= _userOptions.Username;
                await _toDoItemsDbAccessor.AddToDoItem(item);
                _databaseCleanupDriver.DatabaseItemsToRemove.Add(("ToDoItem", item));
            }
        }

        [When(@"user '(.*)' asks the service to return all tasks")]
        public async Task WhenUserAsksTheServiceToReturnAllTasks(string username)
        {
            using var tmpUser = await _authorizationManager.AuthorizeAsUser(username);

            _responseToDoItems = await _toDoItemsDataAccess.GetAllToDoItems();
        }

        [Then(@"response contains the next tasks")]
        public void ThenResponseContainsTheNextTasks(IEnumerable<ToDoItemGetResponse> expectedToDoItems)
        {
            _responseToDoItems.Should()
                .BeEquivalentTo(expectedToDoItems, options => options.Including(item => item.Title)
                    .Including(item => item.Description)
                    .Including(item => item.AssignedTo));
        }
    }
}