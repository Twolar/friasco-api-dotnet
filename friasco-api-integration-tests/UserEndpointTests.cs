using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using friasco_api.Enums;
using friasco_api.Models;

namespace friasco_api_integration_tests;

[TestFixture]
public class UserEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task FirstTestExample()
    {
        var userCreateRequestModel = new UserCreateRequestModel
        {
            Username = "notInDbUser1",
            Email = "notInDbUser@example.com",
            FirstName = "notInDbUser1First",
            LastName = "notInDbUser1Last",
            Role = UserRoleEnum.User,
            Password = "notInDbPassword123",
            ConfirmPassword = "notInDbPassword123"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateRequestModel), Encoding.UTF8, "application/json");

        await Client.PostAsync("/users", jsonContent);

        var response = await Client.GetAsync("/users");

        var users = response.Content.ReadAsStringAsync();

        // TODO: Fix, as this running duplicate entry is still returning okay, need to assert user is created...
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task FirstTestExampleDuplicated()
    {
        var userCreateRequestModel = new UserCreateRequestModel
        {
            Username = "notInDbUser1",
            Email = "notInDbUser@example.com",
            FirstName = "notInDbUser1First",
            LastName = "notInDbUser1Last",
            Role = UserRoleEnum.User,
            Password = "notInDbPassword123",
            ConfirmPassword = "notInDbPassword123"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateRequestModel), Encoding.UTF8, "application/json");

        await Client.PostAsync("/users", jsonContent);

        var response = await Client.GetAsync("/users");

        var users = response.Content.ReadAsStringAsync();

        // TODO: Fix, as this running duplicate entry is still returning okay, need to assert user is created...
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
