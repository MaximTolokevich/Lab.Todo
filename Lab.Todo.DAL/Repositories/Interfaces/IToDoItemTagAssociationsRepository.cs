using Lab.Todo.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories.Interfaces
{
    public interface IToDoItemTagAssociationsRepository
    {
        Task<IEnumerable<TagDbEntry>> GetMostUsedTagsAsync(string usedBy, DateTime? usageDateLowerBound, bool trackEntities = false);
        void Create(ToDoItemTagAssociationDbEntry toDoItemTagAssociation);
        void Update(ToDoItemTagAssociationDbEntry toDoItemTagAssociation);
        void Delete(ToDoItemTagAssociationDbEntry toDoItemTagAssociation);
    }
}
