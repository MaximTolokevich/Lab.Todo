using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Todo.SmokeTests.Services.Interfaces
{
    public interface IDbAccessor 
    {
        Task<int> ExecuteAsync(string sql, object? param = null);
        Task<IEnumerable<T>> SelectAsync<T>(string sql, object? param = null);
    }
}