using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories
{
    public class ToDoItemTagAssociationsRepository : IToDoItemTagAssociationsRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<ToDoItemTagAssociationDbEntry> _table;

        public ToDoItemTagAssociationsRepository(DbContext context)
        {
            _context = context;
            _table = context.Set<ToDoItemTagAssociationDbEntry>();
        }

        public async Task<IEnumerable<TagDbEntry>> GetMostUsedTagsAsync(string usedBy, DateTime? usageDateLowerBound, bool trackEntities = false)
        {
            var query = (usageDateLowerBound == null) ? _table :
                _table.Where(toDoItemTagAssociation => toDoItemTagAssociation.ToDoItem.CreatedDate >= usageDateLowerBound);

            var tags = await query
                .Where(toDoItemTagAssociation => toDoItemTagAssociation.ToDoItem.CreatedBy == usedBy)
                .GroupBy(toDoItemTagAssociation => new { toDoItemTagAssociation.Tag.Id, toDoItemTagAssociation.Tag.Value, toDoItemTagAssociation.Tag.IsPredefined })
                .OrderByDescending(group => group.Count())
                .Select(group => new TagDbEntry { Id = group.Key.Id, Value = group.Key.Value, IsPredefined = group.Key.IsPredefined })
                .ToListAsync();

            if (trackEntities)
            {
                foreach (var tag in tags)
                {
                    TrackTagEntryIfNotTracked(tag);
                }
            }

            return tags;
        }

        public void Create(ToDoItemTagAssociationDbEntry toDoItemTagAssociation) => _table.Add(toDoItemTagAssociation);

        public void Update(ToDoItemTagAssociationDbEntry toDoItemTagAssociation) => _table.Update(toDoItemTagAssociation);

        public void Delete(ToDoItemTagAssociationDbEntry toDoItemTagAssociation) => _table.Remove(toDoItemTagAssociation);

        private void TrackTagEntryIfNotTracked(TagDbEntry tag)
        {
            var isTracked = _context.ChangeTracker.Entries<TagDbEntry>().Any(tagEntry => tagEntry.Entity.Id == tag.Id);

            if (isTracked == false)
            {
                _context.Entry(tag).State = EntityState.Unchanged;
            }
        }
    }
}
