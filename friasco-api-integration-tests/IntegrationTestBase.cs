using System.Data;
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
    protected IDbTransaction? Transaction { get; private set; }

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
        // TODO: The transactions are working but are slow, investigate performance improvement...

        // Open SQL Transaction
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();
            Transaction = connection.BeginTransaction();
        }
    }

    [TearDown]
    public void TearDown()
    {
        // Rollback SQL Transaction and clean up
        if (Transaction != null) {
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction.Connection?.Dispose();
        }
    }
}
