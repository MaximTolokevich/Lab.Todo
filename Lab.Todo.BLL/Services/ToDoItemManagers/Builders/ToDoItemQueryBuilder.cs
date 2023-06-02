using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lab.Todo.DAL.Entities;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Builders
{
    public class ToDoItemQueryBuilder : IToDoItemQueryBuilder
    {
        private readonly ICollection<Expression<Func<ToDoItemDbEntry, bool>>> _filters = new List<Expression<Func<ToDoItemDbEntry, bool>>>();
        private IQueryable<ToDoItemDbEntry> _baseQuery;

        public IToDoItemQueryBuilder SetBaseQuery(IQueryable<ToDoItemDbEntry> baseQuery)
        {
            _baseQuery = baseQuery;
            return this;
        }

        public IToDoItemQueryBuilder AddFilter(Expression<Func<ToDoItemDbEntry, bool>> filter)
        {
            _filters.Add(filter);
            return this;
        }

        public IQueryable<ToDoItemDbEntry> Build()
        {
            return _filters.Aggregate(_baseQuery, (current, filter) => current.Where(filter));
        }
    }
}