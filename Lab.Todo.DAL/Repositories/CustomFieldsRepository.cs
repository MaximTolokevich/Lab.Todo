using System.Linq;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab.Todo.DAL.Repositories
{
    public class CustomFieldsRepository : EfRepository<CustomFieldDbEntry>, ICustomFieldsRepository
    {
        public CustomFieldsRepository(DbContext context) : base(context) { }

        public void DeleteRelatedToToDoItem(int toDoItemId)
        {
            var relatedFields = Table.Where(field => field.ToDoItemId == toDoItemId);
            Table.RemoveRange(relatedFields);
        }
    }
}