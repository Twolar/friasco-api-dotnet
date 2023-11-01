using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    protected JsonSerializerOptions DefaultTestingJsonSerializerOptions { get; set; }
    protected string FriascoTestDatabaseString { get; private set;}

    public IntegrationTestBase()
    {
        var friascoTestDatabasePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "FriascoDatabaseTEST.db");
        FriascoTestDatabaseString = $"Data Source={friascoTestDatabasePath}";

        DefaultTestingJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        DefaultTestingJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

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
                        return new DataContext(() => new SqliteConnection(FriascoTestDatabaseString));
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
    }

    [TearDown]
    public void TearDown()
    {
    }

    #region Helpers

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
