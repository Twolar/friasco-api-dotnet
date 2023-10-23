using System.Data;
using Dapper;

namespace friasco_api.Data;

public interface IDataContext
{
    Task InitDatabase();
    IDbConnection CreateConnection();
}

public class DataContext : IDataContext
{
    private readonly Func<IDbConnection> _dbConnectionFunc;

    public DataContext(Func<IDbConnection> dbConnectionFunc)
    {
        _dbConnectionFunc = dbConnectionFunc;
    }

    public IDbConnection CreateConnection()
    {
        return _dbConnectionFunc.Invoke();
    }

    public async Task InitDatabase()
    {
        using (var connection = _dbConnectionFunc())
        {
            await _initUsers();

            async Task _initUsers()
            {
                var sql = @"
                    CREATE TABLE IF NOT EXISTS 
                    Users (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Username TEXT,
                        FirstName TEXT,
                        LastName TEXT,
                        Email TEXT,
                        Role INTEGER,
                        PasswordHash TEXT
                    );
                ";

                await connection.ExecuteAsync(sql);
            }
        }
    }
}
