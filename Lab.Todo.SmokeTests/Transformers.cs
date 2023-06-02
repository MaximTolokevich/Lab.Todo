using System.Collections.Generic;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.SmokeTests.DatabaseModels;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Lab.Todo.SmokeTests
{
    [Binding]
    public class Transformers
    {
        [StepArgumentTransformation]
        public IEnumerable<ToDoItemDbModel> TransformTableToToDoItemDbModel(Table toDoItems) => toDoItems.CreateSet<ToDoItemDbModel>();

        [StepArgumentTransformation]
        public IEnumerable<ToDoItemGetResponse> TransformTableToToDoItemGetResponse(Table toDoItems) => toDoItems.CreateSet<ToDoItemGetResponse>();
    }
}