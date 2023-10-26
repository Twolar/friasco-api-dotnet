using System.Text;
using System.Text.Json;
using friasco_api.Enums;
using friasco_api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace friasco_api_integration_tests;

[TestFixture]
public class Test
{
    [SetUp]
    public void SetUp()
    {

    }

    [Test]
    public async Task TestTemporary()
    {
        // TODO: Seperate database for integration tests either by :inmemory: or standalone
        var factory = new WebApplicationFactory<Program>();

        var client = factory.CreateClient();

        var userCreateRequestModel = new UserCreateRequestModel
        {
            Username = "intUser1",
            Email = "user@example.com",
            FirstName = "intuser1First",
            LastName = "intuser1Last",
            Role = UserRoleEnum.User,
            Password = "intpassword123",
            ConfirmPassword = "intpassword123"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateRequestModel), Encoding.UTF8, "application/json");

        await client.PostAsync("/users", jsonContent);

        var response = await client.GetAsync("/users");

        var users = response.Content.ReadAsStringAsync();

        Console.WriteLine(users);
    }
}
