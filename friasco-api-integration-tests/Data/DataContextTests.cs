using friasco_api.Data;
using Microsoft.Data.Sqlite;
using friasco_api_integration_tests;

namespace friasco_api_test.Data;

[TestFixture]
public class DataContextTests : IntegrationTestBase
{
    private string _connectionString = "Data Source=sharedmemdb;Mode=Memory;Cache=Shared";
    private DataContext _dataContext;

    [SetUp]
    public void SetUp()
    {
        _dataContext = new DataContext(() => new SqliteConnection(_connectionString));
    }

    [Test]
    public async Task InitDatabase_CreatesUsersTable()
    {
        await _dataContext.InitDatabase();

        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Users';";
                var tableExists = await command.ExecuteScalarAsync();

                Assert.That(tableExists, Is.EqualTo(1));
            }
        }
    }


    [Test]
    public async Task InitDatabase_CreatesCorrectColumns()
    {
        await _dataContext.InitDatabase();

        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(Users);";
                var reader = await command.ExecuteReaderAsync();

                var columns = new List<string>();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(1)); // Column name is in the second column of the result set
                }

                Assert.That(columns, Does.Contain("Id"));
                Assert.That(columns, Does.Contain("Username"));
                Assert.That(columns, Does.Contain("FirstName"));
                Assert.That(columns, Does.Contain("LastName"));
                Assert.That(columns, Does.Contain("Email"));
                Assert.That(columns, Does.Contain("Role"));
                Assert.That(columns, Does.Contain("PasswordHash"));
            }
        }
    }
}