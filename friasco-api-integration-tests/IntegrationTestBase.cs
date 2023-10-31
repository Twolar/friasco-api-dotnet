using System.Data;
using System.Text.Json;
using Dapper;
using friasco_api.Data;
using friasco_api.Data.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace friasco_api_integration_tests;

[TestFixture]
public class IntegrationTestBase
{
    protected WebApplicationFactory<Program> Factory { get; private set; }
    protected HttpClient Client { get; private set; }
    protected IDbTransaction? Transaction { get; private set; }
    protected JsonSerializerOptions DefaultTestingJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false
    };

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove Friasco_Api DbContext DI
                    var apiDbContextDescription = services.SingleOrDefault(d => d.ServiceType == typeof(IDataContext));
                    if (apiDbContextDescription != null)
                    {
                        services.Remove(apiDbContextDescription);
                    }

                    // Replace with our own for testing purposes
                    services.AddScoped<IDataContext, DataContext>(serviceProvider =>
                    {
                        var testingDbPathAndName = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "FriascoDatabaseTEST.db");
                        return new DataContext(() => new SqliteConnection($"Data Source={testingDbPathAndName}"));
                    });
                });
            });

        Client = Factory.CreateClient();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            await context.InitDatabase();
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        // FUTURE TODO: Causing locks in SQLite, potentially revisit when migrated to different database
        // Open SQL Transaction
        // using (var scope = Factory.Services.CreateScope())
        // {
        //     var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
        //     var connection = context.CreateConnection();
        //     connection.Open();
        //     Transaction = connection.BeginTransaction();
        // }
    }

    [TearDown]
    public void TearDown()
    {
        // FUTURE TODO: Causing locks in SQLite, potentially revisit when migrated to different database
        // // Rollback SQL Transaction and clean up
        // if (Transaction != null)
        // {
        //     Transaction.Rollback();
        //     Transaction.Dispose();
        //     Transaction.Connection?.Dispose();
        // }
    }

    #region Helpers

    public async Task<string> GetResponseResultObjectAsString(HttpResponseMessage response)
    {
        string responseContent = await response.Content.ReadAsStringAsync();
        using JsonDocument jsonDocument = JsonDocument.Parse(responseContent);
        JsonElement resultJsonElement = jsonDocument.RootElement.GetProperty("result");
        return resultJsonElement.GetRawText();
    }

    public async Task<int> DbUserCreate(User user)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    INSERT INTO Users 
                    (
                        Username,
                        FirstName,
                        LastName,
                        Email,
                        Role,
                        PasswordHash
                    )
                    VALUES 
                    (
                        @Username,
                        @FirstName,
                        @LastName,
                        @Email,
                        @Role,
                        @PasswordHash
                    )
                ";
                return await connection.ExecuteAsync(sql, user);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task<User?> DbUserGetById(int id)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    SELECT * FROM Users
                    WHERE Id = @id
                ";
                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { id });
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task<User?> DbUserGetByEmail(string email)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    SELECT * FROM Users
                    WHERE Email = @email
                ";
                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { email });
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task DbUserDeleteById(int id)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    DELETE FROM Users 
                    WHERE Id = @id
                ";
                await connection.ExecuteAsync(sql, new { id });
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public async Task DbUserDeleteByEmail(string email)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    DELETE FROM Users 
                    WHERE Email = @email
                ";
                await connection.QueryFirstOrDefaultAsync<User>(sql, new { email });
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    #endregion
}
