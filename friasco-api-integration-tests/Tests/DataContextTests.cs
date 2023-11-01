using Dapper;
using friasco_api.Data;
using Microsoft.Extensions.DependencyInjection;

namespace friasco_api_integration_tests.Tests;

[TestFixture]
public class DataContextTests : IntegrationTestBase
{
    [Test]
    public async Task InitDatabase_CreatesUsersTable()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    SELECT count(*) 
                    FROM sqlite_master 
                    WHERE type='table' AND name='Users'
                ";
                var tableCount = await connection.ExecuteScalarAsync<int>(sql);

                Assert.That(tableCount, Is.EqualTo(1));
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }


    [Test]
    public async Task InitDatabase_CreatesCorrectColumns()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var connection = context.CreateConnection();
            connection.Open();

            try
            {
                var sql = @"
                    PRAGMA table_info(Users)
                ";
                var columns = await connection.QueryAsync<dynamic>(sql);
                var columnNames = columns.Select(c => (string)c.name).ToList();

                Assert.That(columnNames, Does.Contain("Id"));
                Assert.That(columnNames, Does.Contain("Username"));
                Assert.That(columnNames, Does.Contain("FirstName"));
                Assert.That(columnNames, Does.Contain("LastName"));
                Assert.That(columnNames, Does.Contain("Email"));
                Assert.That(columnNames, Does.Contain("Role"));
                Assert.That(columnNames, Does.Contain("PasswordHash"));
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}