using System.Data;
using friasco_api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace friasco_api_integration_tests;

[TestFixture]
public class IntegrationTestBase
{
    private WebApplicationFactory<Program> _factory;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    #region Helpers

    protected async Task<HttpClient> GetHttpClientAsync()
    {
        // TODO: Look at potentially refactoring this again? i.e. Do we have to do all this everytime we want an HTTP Cleint?
        var client = _factory.WithWebHostBuilder(builder =>
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
        }).CreateClient();

        // Ensure database and tables exist
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            await context.InitDatabase();
        }

        return client;
    }

    #endregion
}
