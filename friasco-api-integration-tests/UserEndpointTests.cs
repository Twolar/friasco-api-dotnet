using System.Net;
using System.Text;
using System.Text.Json;
using friasco_api.Data.Entities;
using friasco_api.Enums;

namespace friasco_api_integration_tests;

[TestFixture]
public class UserEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task Users_GetAll_Succeeds_WithMatchingDbValues()
    {
        var dbUserList = new List<User>() {
            new User
            {
                Username = "User1",
                Email = "User1@example.com",
                FirstName = "User1First",
                LastName = "User1Last",
                Role = UserRoleEnum.User,
                PasswordHash = "PasswordHash123",
            },
            new User
            {
                Username = "User2",
                Email = "User2@example.com",
                FirstName = "User2First",
                LastName = "User2Last",
                Role = UserRoleEnum.User,
                PasswordHash = "PasswordHash123",
            }
        };

        try
        {
            await DbUserCreate(dbUserList[0]);
            await DbUserCreate(dbUserList[1]);

            var response = await Client.GetAsync("/users");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            // TODO: Deserialize response, potentially look at changing response structure first...
            // var apiUsers = JsonSerializer.Deserialize();

            // Assert.That(apiUsers, Is.Not.EqualTo(null));

            // for (var i = 0; i < dbUserList.Count; i++)
            // {
            //     Assert.That(apiUsers[i].Username, Is.EqualTo(dbUserList[i].Username));
            //     Assert.That(apiUsers[i].Email, Is.EqualTo(dbUserList[i].Email));
            //     Assert.That(apiUsers[i].FirstName, Is.EqualTo(dbUserList[i].FirstName));
            //     Assert.That(apiUsers[i].LastName, Is.EqualTo(dbUserList[i].LastName));
            //     Assert.That(apiUsers[i].Role, Is.EqualTo(dbUserList[i].Role));
            // }
        }
        finally
        {
            await DbUserDeleteByEmail(dbUserList[1].Email);
            await DbUserDeleteByEmail(dbUserList[0].Email);
        }
    }

    [Test]
    public async Task Users_Get_Succeeds_WithMatchingDbValues() { }

    [Test]
    public async Task Users_Create_Succeeds_WithMatchingDbValues()
    {
        User? userInDb = null;

        try
        {
            var userCreateJsonObject = new
            {
                Username = "User1",
                Email = "User1@example.com",
                FirstName = "User1First",
                LastName = "User1Last",
                Role = UserRoleEnum.User,
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/users", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            userInDb = await DbUserGetByEmail(userCreateJsonObject.Email);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(userCreateJsonObject.Username));
            Assert.That(userInDb.Email, Is.EqualTo(userCreateJsonObject.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(userCreateJsonObject.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(userCreateJsonObject.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(userCreateJsonObject.Role));
            Assert.That(userInDb.PasswordHash, Is.Not.EqualTo(null));
        }
        finally
        {
            if (userInDb != null)
            {
                await DbUserDeleteById(userInDb.Id);
            }
        }
    }

    [Test]
    public async Task Users_Update_Succeeds_WithMatchingDbValues() { }

    [Test]
    public async Task Users_Delete_Succeeds_WithMatchingDbValues() { }
}
