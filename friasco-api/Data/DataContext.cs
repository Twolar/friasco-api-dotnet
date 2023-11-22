using System.Data;
using Dapper;
using friasco_api.Helpers;

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
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

            SqlMapper.AddTypeHandler(new GuidTypeHandlerHelper());

            await _initUsers();
            await _initRefreshTokens();

            async Task _initUsers()
            {
                var sql = @"
                    CREATE TABLE IF NOT EXISTS 
                    Users (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE,
                        FirstName TEXT,
                        LastName TEXT,
                        Email TEXT UNIQUE,
                        Role INTEGER,
                        PasswordHash TEXT,
                        Guid BLOB UNIQUE NOT NULL
                    );
                ";

                await connection.ExecuteAsync(sql);
            }

            async Task _initRefreshTokens()
            {
                var sql = @"
                    CREATE TABLE IF NOT EXISTS 
                    RefreshTokens (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        UserGuid BLOB,
                        JwtId TEXT UNIQUE,
                        Token TEXT UNIQUE,
                        ExpirationDate DATETIME,
                        CreatedDate DATETIME,
                        IsUsed BOOLEAN NOT NULL DEFAULT 0 CHECK (IsUsed IN (0, 1)),
                        IsValid BOOLEAN NOT NULL DEFAULT 1 CHECK (IsValid IN (0, 1)),
                        FOREIGN KEY(UserGuid) REFERENCES Users(Guid) ON DELETE CASCADE
                    );
                ";

                // TODO: After migration to another database, look at changing the token to be the primary key/id

                await connection.ExecuteAsync(sql);
            }
        }
    }
}
