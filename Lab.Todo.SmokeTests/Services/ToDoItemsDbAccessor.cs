using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Lab.Todo.SmokeTests.Options;
using Lab.Todo.SmokeTests.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Lab.Todo.SmokeTests.Services
{
    public class ToDoItemsDbAccessor : IToDoItemsDbAccessor, IDisposable
    {
        private readonly DatabaseOptions _databaseOptions;
        private SqlConnection? _sqlConnection;

        public ToDoItemsDbAccessor(IOptions<DatabaseOptions> databaseOptions)
        {
            _databaseOptions = databaseOptions.Value;
        }

        private SqlConnection DatabaseConnection => _sqlConnection ??= new SqlConnection(_databaseOptions.ConnectionString);

        public async Task<int> ExecuteAsync(string sql, object? param = null) => await DatabaseConnection.ExecuteAsync(sql, param);

        public async Task<IEnumerable<T>> SelectAsync<T>(string sql, object? param = null) => await DatabaseConnection.QueryAsync<T>(sql, param);

        public void Dispose() => _sqlConnection?.Dispose();
    }
}