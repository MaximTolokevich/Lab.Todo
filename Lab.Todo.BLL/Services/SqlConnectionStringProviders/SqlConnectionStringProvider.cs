using System;
using System.Data.SqlClient;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lab.Todo.BLL.Services.SqlConnectionStringProviders
{
    public class SqlConnectionStringProvider : ISqlConnectionStringProvider
    {
        private readonly SqlConnectionStringOptions _options;
        private readonly ILogger<SqlConnectionStringProvider> _logger;

        public SqlConnectionStringProvider(IOptions<SqlConnectionStringOptions> options, ILogger<SqlConnectionStringProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string GetSqlDatabaseConnectionString()
        {
            var builder = new SqlConnectionStringBuilder();

            if (string.IsNullOrEmpty(_options.DataSource))
            {
                throw new ArgumentException("Data Source for connection string can't be empty or null.");
            }
            builder.DataSource = _options.DataSource;

            if (string.IsNullOrEmpty(_options.InitialCatalog))
            {
                throw new ArgumentException("Initial Catalog for connection string can't be empty or null.");
            }
            builder.InitialCatalog = _options.InitialCatalog;

            var useWindowsAuthentication = true;
            string logMessage = "SqlConnectionString successfully created.";

            if (!string.IsNullOrEmpty(_options.UserId) &&
                !string.IsNullOrEmpty(_options.Password))
            {
                builder.UserID = _options.UserId;
                builder.Password = _options.Password;
                useWindowsAuthentication = false;

                logMessage += $" {nameof(builder.UserID)} and {nameof(builder.Password)} were found in options.";
            }

            builder.IntegratedSecurity = useWindowsAuthentication;

            _logger.LogDebug(logMessage);

            return builder.ToString();
        }
    }
}