using friasco_api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace friasco_api_integration_tests;

[TestFixture]
public class IntegrationTestBase
{
    protected WebApplicationFactory<Program> Factory { get; private set; }
    protected HttpClient Client { get; private set; }

    [SetUp]
    public async Task SetUp()
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

    [TearDown]
    public void TearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
