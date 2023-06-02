using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Lab.Todo.DAL.Repositories
{
    public class ToDoItemDependenciesRepository : IToDoItemDependenciesRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<ToDoItemDependencyDbEntry> _table;

        public ToDoItemDependenciesRepository(DbContext context)
        {
            _context = context;
            _table = context.Set<ToDoItemDependencyDbEntry>();
        }

        public virtual void Delete(ToDoItemDependencyDbEntry entity) => _table.Remove(entity);

        public async Task<IEnumerable<CyclicDependencyDbEntry>> GetCyclicDependenciesAsync(int toDoItemId,
            IEnumerable<int> dependenciesIds)
        {
            var tableSchema = new SqlMetaData("Id", SqlDbType.Int);
            var idsTable = dependenciesIds.Select(dependencyId =>
            {
                var tableRow = new SqlDataRecord(tableSchema);
                tableRow.SetInt32(0, dependencyId);

                return tableRow;
            });
            var parameters = new List<SqlParameter>
            {
                new() { ParameterName = "@toDoItemId", SqlDbType = SqlDbType.Int, Value = toDoItemId },
                new()
                {
                    ParameterName = "@dependenciesIds", SqlDbType = SqlDbType.Structured, TypeName = "dbo.IDs",
                    Value = idsTable
                }
            };

            return (await _context.Set<CyclicDependencyNodeDbEntry>()
                    .FromSqlInterpolated($"EXECUTE dbo.FindCyclicDependencies {parameters[0]}, {parameters[1]}")
                    .ToListAsync())
                .GroupBy(cyclicDependencyNode => cyclicDependencyNode.CycleId)
                .Select(group => new CyclicDependencyDbEntry
                    { IdsCycle = group.Select(cyclicDependencyNode => cyclicDependencyNode.DependencyId) });
        }

        public async Task<IEnumerable<CyclicDependencyDbEntry>> GetCyclicParentTaskAsync(int toDoItemId,
            int parentTaskId)
        {
            var parameters = new List<SqlParameter>
            {
                new() { ParameterName = "@toDoItemId", SqlDbType = SqlDbType.Int, Value = toDoItemId },
                new() { ParameterName = "@parentTaskId", SqlDbType = SqlDbType.Int, Value = parentTaskId }
            };

            var cyclicDependencyDbEntries = (await _context.Set<CyclicDependencyNodeDbEntry>()
                    .FromSqlInterpolated($"EXECUTE dbo.FindCyclicParentTasks {parameters[0]}, {parameters[1]}")
                    .ToListAsync())
                .GroupBy(cyclicDependencyNode => cyclicDependencyNode.CycleId)
                .Select(group => new CyclicDependencyDbEntry
                    { IdsCycle = group.Select(cyclicDependencyNode => cyclicDependencyNode.DependencyId) });

            return toDoItemId == parentTaskId
                ? cyclicDependencyDbEntries.Append(new CyclicDependencyDbEntry
                    { IdsCycle = new[] { toDoItemId, parentTaskId } })
                : cyclicDependencyDbEntries;
        }
    }
}