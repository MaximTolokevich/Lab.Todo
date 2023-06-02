using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories
{
    public class TagsRepository : EfRepository<TagDbEntry>, ITagsRepository
    {
        public TagsRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<TagDbEntry>> GetPredefinedAsync(bool trackEntities = false)
        {
            var query = (trackEntities == false) ? Table.AsNoTracking() : Table;

            return await query.Where(tag => tag.IsPredefined == true).ToListAsync();
        }

        public async Task<IEnumerable<TagDbEntry>> GetByValuesAsync(IEnumerable<string> values, bool trackEntity = false)
        {
            var query = (trackEntity == false) ? Table.AsNoTracking() : Table;

            return await query.Where(tag => values.Contains(tag.Value)).ToListAsync();
        }
    }
}
