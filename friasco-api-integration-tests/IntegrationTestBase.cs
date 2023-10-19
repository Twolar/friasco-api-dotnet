using Dapper;
using Microsoft.Data.Sqlite;

namespace friasco_api_integration_tests;

public class IntegrationTestBase
{
    #region Helper Methods

    public async Task AddUserSqlite(
        SqliteConnection connection,
        string username,
        string firstName,
        string lastName,
        string email,
        int role,
        string passwordHash)
    {
        string sql = @"
        INSERT INTO Users (Username, FirstName, LastName, Email, Role, PasswordHash) 
        VALUES (@Username, @FirstName, @LastName, @Email, @Role, @PasswordHash);
        ";

        var user = new
        {
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role,
            PasswordHash = passwordHash
        };

        await connection.ExecuteAsync(sql, user);
    }


    #endregion
}
