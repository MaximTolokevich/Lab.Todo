using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Lab.Todo.SmokeTests.Helpers
{
    public static class DatabaseInfo
    {
        public static string GetTableName<T>()
        {
            var tableAttribute = typeof(T)
                .GetCustomAttribute<TableAttribute>(false);

            return tableAttribute?.Name ?? typeof(T).Name;
        }
    }
}