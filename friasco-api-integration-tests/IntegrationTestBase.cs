using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace friasco_api_integration_tests;

public class IntegrationTestBase
{
    private WebApplicationFactory<Program> _factory;
    
    #region Helper Methods

    public HttpClient CreateHttpClient()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // TODO: Add services here...
                });
            });
        return _factory.CreateClient();
    }

    public void CleanUpHttpClient(HttpClient client)
    {
        client.Dispose();
        _factory.Dispose();
    }

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
