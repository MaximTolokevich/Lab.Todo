using Lab.Todo.BLL.Services.SqlConnectionStringProviders.Options;
using Microsoft.Extensions.Configuration;

namespace Lab.Todo.BLL.Services.SqlConnectionStringProviders.ConfigurationExtensions
{
    public static class ConfigurationExtension
    {
        public static void BuildSqlConnectionStringOptions(this IConfiguration configuration, SqlConnectionStringOptions options)
        {
            options.DataSource = configuration[ConfigurationKeys.DataSource];
            options.InitialCatalog = configuration[ConfigurationKeys.InitialCatalog];
            options.UserId = configuration[ConfigurationKeys.UserId];
            options.Password = configuration[ConfigurationKeys.Password];
        }
    }
}