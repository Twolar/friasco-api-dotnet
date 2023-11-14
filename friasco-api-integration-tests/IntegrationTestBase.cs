using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using friasco_api;
using friasco_api.Data;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Helpers;
using friasco_api_integration_tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace friasco_api_integration_tests;

[TestFixture]
public class IntegrationTestBase
{
    protected WebApplicationFactory<Program> Factory { get; private set; }
    protected IDbTransaction? Transaction { get; private set; }
    protected JsonSerializerOptions DefaultTestingJsonSerializerOptions { get; set; }
    protected string FriascoTestDatabaseString { get; private set; }
    protected HttpClient ApiClientWithNoAuth { get; private set; }
    protected HttpClient ApiClientWithRoleUser { get; private set; }
    protected HttpClient ApiClientWithRoleAdmin { get; private set; }
    protected HttpClient ApiClientWithRoleSuperAdmin { get; private set; }
    protected static List<string> ApiClientNames = new List<string> {
        nameof(ApiClientWithNoAuth),
        nameof(ApiClientWithRoleUser),
        nameof(ApiClientWithRoleAdmin),
        nameof(ApiClientWithRoleSuperAdmin)
    };

    public IntegrationTestBase()
    {
        var friascoTestDatabasePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "FriascoDatabaseTEST.db");
        FriascoTestDatabaseString = $"Data Source={friascoTestDatabasePath}";

        // Setup dummy data to use for testing environment variables
        Environment.SetEnvironmentVariable("JWT_KEY", "F6MTF6jJ5I013c3tfXd0O+pw5QUsdCv/8v+v1KLTQjlw1amYAsFb9DqNvKLVpsFs");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_SECONDS", "60");
        Environment.SetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS", "1");

        DefaultTestingJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
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

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            await context.InitDatabase();
        }

        await InitializeAllApiClients();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        ApiClientWithNoAuth.Dispose();
        ApiClientWithRoleUser.Dispose();
        ApiClientWithRoleAdmin.Dispose();
        ApiClientWithRoleSuperAdmin.Dispose();
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

    public async Task InitializeAllApiClients()
    {
        ApiClientWithNoAuth = Factory.CreateClient();
        ApiClientWithRoleUser = await CreateAuthenticatedHttpClient("UserRole@example.com", "Password123");
        ApiClientWithRoleAdmin = await CreateAuthenticatedHttpClient("AdminRole@example.com", "Password123");
        ApiClientWithRoleSuperAdmin = await CreateAuthenticatedHttpClient("SuperAdminRole@example.com", "Password123");
    }

    public async Task<HttpClient> CreateAuthenticatedHttpClient(string userEmail, string userPassword)
    {
        var client = Factory.CreateClient();

        var apiUserLoginObject = new
        {
            Email = userEmail,
            Password = userPassword,
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(apiUserLoginObject), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/Auth/Login", jsonContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

        var contentJsonString = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<AuthResponseModel>(contentJsonString, DefaultTestingJsonSerializerOptions);
        Assert.That(loginResponse.Token, Is.Not.EqualTo(string.Empty));
        Assert.That(loginResponse.Token, Is.Not.EqualTo(null));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        return client;
    }

    public async Task DbUserCreate(User user)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var bcryptWrapper = scope.ServiceProvider.GetRequiredService<IBCryptWrapper>();

            try
            {
                user.Guid = Guid.NewGuid();
                user.PasswordHash = bcryptWrapper.HashPassword(user.PasswordHash);
                await userRepository.Create(user);
            }
            finally
            {
            }
        }
    }

    public async Task<User?> DbUserGetById(int id)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                return await userRepository.GetById(id);
            }
            finally
            {
            }
        }
    }

    public async Task<User?> DbUserGetByEmail(string email)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                return await userRepository.GetByEmail(email);
            }
            finally
            {
            }
        }
    }

    public async Task DbUserDeleteById(int id)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                var userToDelete = userRepository.GetById(id);
                if (userToDelete != null)
                {
                    await userRepository.Delete(id);
                }
            }
            finally
            {
            }
        }
    }

    public async Task DbUserDeleteByEmail(string email)
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                var userToDelete = await userRepository.GetByEmail(email);
                if (userToDelete != null)
                {
                    await userRepository.Delete(userToDelete.Id);
                }
            }
            finally
            {
            }
        }
    }

    #endregion

    // TODO: Add method to clean up Refresh Tokens?
}
