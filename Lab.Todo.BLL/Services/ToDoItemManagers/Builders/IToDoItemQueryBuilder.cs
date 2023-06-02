using System;
using System.Linq;
using System.Linq.Expressions;
using Lab.Todo.DAL.Entities;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Builders
{
    public interface IToDoItemQueryBuilder
    {
        IToDoItemQueryBuilder SetBaseQuery(IQueryable<ToDoItemDbEntry> baseQuery);
        IToDoItemQueryBuilder AddFilter(Expression<Func<ToDoItemDbEntry, bool>> filter);
        IQueryable<ToDoItemDbEntry> Build();
    }
}