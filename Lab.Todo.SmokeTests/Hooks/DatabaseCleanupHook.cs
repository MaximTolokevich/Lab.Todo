using System.Threading.Tasks;
using Lab.Todo.SmokeTests.Drivers;
using Lab.Todo.SmokeTests.Helpers;
using Lab.Todo.SmokeTests.Services.Interfaces;
using TechTalk.SpecFlow;

namespace Lab.Todo.SmokeTests.Hooks
{
    [Binding]
    public class DatabaseCleanupHook
    {
        private readonly IToDoItemsDbAccessor _toDoItemsDbAccessor;
        private readonly DatabaseCleanupDriver _databaseCleanupDriver;

        public DatabaseCleanupHook(IToDoItemsDbAccessor toDoItemsDbAccessor, DatabaseCleanupDriver databaseCleanupDriver)
        {
            _toDoItemsDbAccessor = toDoItemsDbAccessor;
            _databaseCleanupDriver = databaseCleanupDriver;
        }

        [AfterScenario]
        public async Task CleanTestedDatabaseItems()
        {
            foreach (var (tableName, databaseEntity) in _databaseCleanupDriver.DatabaseItemsToRemove)
            {
                await _toDoItemsDbAccessor.DeleteByFilterAsync(tableName, databaseEntity);
            }
        }
    }
}