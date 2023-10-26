using System.Text;
using System.Text.Json;
using friasco_api.Enums;
using friasco_api.Models;

namespace friasco_api_integration_tests;

[TestFixture]
public class UserEndpointTests : IntegrationTestBase
{
    [SetUp]
    public void SetUp()
    {

    }

    [TearDown]
    public void TearDown()
    {

    }

    [Test]
    public async Task FirstTestExample()
    {
        var client = await GetHttpClientAsync();

        try
        {
            var userCreateRequestModel = new UserCreateRequestModel
            {
                Username = "tempUser1",
                Email = "tempuser@example.com",
                FirstName = "tempuser1First",
                LastName = "tempuser1Last",
                Role = UserRoleEnum.User,
                Password = "temppassword123",
                ConfirmPassword = "temppassword123"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateRequestModel), Encoding.UTF8, "application/json");

            await client.PostAsync("/users", jsonContent);

            var response = await client.GetAsync("/users");

            var users = response.Content.ReadAsStringAsync();
        }
        finally
        {
            // TODO: Figure out a way to clean up data created
            // Through either helper methods running sql or transactions (transactions if possible...)
        }
    }
}
