using System.Data;
using Dapper;

namespace friasco_api.Data;

// This class and interface is needed so that we can create mocks for testing...
// Otherwise get errors when trying to mock Dapper extension methods.

public interface IDapperWrapper
{
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql);
    Task<T> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql);
    Task<T> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql, object param = null);
    Task<int> ExecuteAsync(IDbConnection connection, string sql, object param = null);
}

public class DapperWrapper : IDapperWrapper
{
    public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql)
    {
        return await connection.QueryAsync<T>(sql);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql)
    {
        return await connection.QueryFirstOrDefaultAsync<T>(sql);
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string sql, object param = null)
    {
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(IDbConnection connection, string sql, object param = null)
    {
        return await connection.ExecuteAsync(sql, param);
    }
}