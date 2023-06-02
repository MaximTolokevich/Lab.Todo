using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace Lab.Todo.SmokeTests.Drivers
{
    [Binding]
    public class DatabaseCleanupDriver
    {
        public ICollection<(string tableName, object databaseEntity)> DatabaseItemsToRemove { get; } 

        public DatabaseCleanupDriver()
        {
            DatabaseItemsToRemove = new List<(string, object)>();
        }
    }
}