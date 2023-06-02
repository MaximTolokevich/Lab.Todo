using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab.Todo.DAL.Repositories
{
    public class ToDoItemStatusesRepository : EfRepository<ToDoItemStatusDbEntry>, IToDoItemStatusesRepository
    {
        public ToDoItemStatusesRepository(DbContext context) : base(context) { }
    }
}