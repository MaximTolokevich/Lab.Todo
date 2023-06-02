using FluentAssertions;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.SmokeTests.Services.DataAccess;
using Lab.Todo.SmokeTests.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Lab.Todo.SmokeTests.Steps
{
    [Binding]
    public class GetAllToDoItemStatusesSteps
    {
        private readonly IToDoItemsService _toDoItemsDataAccess;
        private readonly IAuthorizationManager _authorization;
        private IEnumerable<ToDoItemStatusGetResponse>? statuses;
        public GetAllToDoItemStatusesSteps(IAuthorizationManager authorizationManager, IToDoItemsService toDoItemsData)
        {
            _authorization = authorizationManager;
            _toDoItemsDataAccess = toDoItemsData;

        }
        [Given(@"a test user is successfully logged into an API")]
        public void GivenUserSuccessfullyLoggedApi(IEnumerable<AuthenticateResponse> users)
        {
            using var user = _authorization.AuthorizeAsUser(users.FirstOrDefault().Username);
        }

        [When(@"test user receive all ToDoItem statuses")]
        public async Task UserReceiveAllToDoItemStatuses()
        {
            statuses = await _toDoItemsDataAccess.GetToDoItemStatuses();
        }

        [Then(@"the response contains")]
        public void ResponseContainsTheNextTasks(IEnumerable<ToDoItemStatusGetResponse> taskStatuses)
        {
            taskStatuses.Should().Contain(x => x.StatusName.Equals(ToDoItemStatus.Planned));
            taskStatuses.Should().Contain(x => x.StatusName.Equals(ToDoItemStatus.Cancelled));
            taskStatuses.Should().Contain(x => x.StatusName.Equals(ToDoItemStatus.Completed));
            taskStatuses.Should().Contain(x => x.StatusName.Equals(ToDoItemStatus.InProgress));
            taskStatuses.Should().Contain(x => x.StatusName.Equals(ToDoItemStatus.Paused));

        }
    }
}
