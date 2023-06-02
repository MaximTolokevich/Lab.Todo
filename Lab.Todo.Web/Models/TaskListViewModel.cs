using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using System.Collections.Generic;

namespace Lab.Todo.Web.Models
{
    public class TaskListViewModel
    {
        public IEnumerable<ToDoItemViewModel> AllTasks { get; set; }
        public IEnumerable<ToDoItemStatus> AllStatuses { get; set; }
    }
}