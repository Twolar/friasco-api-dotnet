using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using friasco_api.Data.Entities;
using friasco_api.Enums;

namespace friasco_api_integration_tests;

[TestFixture]
public class UserEndpointTests : IntegrationTestBase
{
    #region Users Get Tests

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

            var resultJsonString = await GetResponseResultObjectAsString(response);
            List<User> apiUsers = JsonSerializer.Deserialize<List<User>>(resultJsonString, DefaultTestingJsonSerializerOptions);

            Assert.That(apiUsers?.Count, Is.AtLeast(2));

            for (var i = 0; i < dbUserList.Count; i++)
            {
                Assert.That(apiUsers[i].Username, Is.EqualTo(dbUserList[i].Username));
                Assert.That(apiUsers[i].Email, Is.EqualTo(dbUserList[i].Email));
                Assert.That(apiUsers[i].FirstName, Is.EqualTo(dbUserList[i].FirstName));
                Assert.That(apiUsers[i].LastName, Is.EqualTo(dbUserList[i].LastName));
                Assert.That(apiUsers[i].Role, Is.EqualTo(dbUserList[i].Role));
                Assert.That(apiUsers[i].PasswordHash, Is.EqualTo(null));
            }
        }
        finally
        {
            await DbUserDeleteByEmail(dbUserList[1].Email);
            await DbUserDeleteByEmail(dbUserList[0].Email);
        }
    }

    [Test]
    public async Task Users_Get_Succeeds_WithMatchingDbValues()
    {
        var newUser = new User
        {
            Username = "User1",
            Email = "ugsUser1@example.com",
            FirstName = "User1First",
            LastName = "User1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "PasswordHash123",
        };

        try
        {
            await DbUserCreate(newUser);

            var userInDb = await DbUserGetByEmail(newUser.Email);
            Assert.That(userInDb, Is.Not.EqualTo(null));

            var response = await Client.GetAsync($"/users/{userInDb.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var resultJsonString = await GetResponseResultObjectAsString(response);
            User apiUserGet = JsonSerializer.Deserialize<User>(resultJsonString, DefaultTestingJsonSerializerOptions);

            Assert.That(apiUserGet, Is.Not.EqualTo(null));
            Assert.That(apiUserGet.Username, Is.EqualTo(userInDb.Username));
            Assert.That(apiUserGet.Email, Is.EqualTo(userInDb.Email));
            Assert.That(apiUserGet.FirstName, Is.EqualTo(userInDb.FirstName));
            Assert.That(apiUserGet.LastName, Is.EqualTo(userInDb.LastName));
            Assert.That(apiUserGet.Role, Is.EqualTo(userInDb.Role));
            Assert.That(apiUserGet.PasswordHash, Is.EqualTo(null));

        }
        finally
        {
            await DbUserDeleteByEmail(newUser.Email);
        }
    }

    [Test]
    public async Task Users_Get_Succeeds_WhenUserIdDoesNotExist()
    {
        var id = 2385942;

        var response = await Client.GetAsync($"/users/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    #endregion

    #region User Create Tests

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
    public async Task Users_Create_Succeeds_WithRandomPropertiesInRequest()
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
                ConfirmPassword = "Password123",
                RandomPropertyOne = "RandomPropertyOne",
                RandomPropertyTwo = "RandomPropertyTwo",
                RandomPropertyThree = "RandomPropertyThree"
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

    // Test: Create user with missing fields
    // Test: Create duplicate user or user with same email
    // Test: Create user with non-matching passwords

    #endregion

    #region Users Update Tests

    [Test]
    public async Task Users_Update_Succeeds_WithMatchingDbValues()
    {
        User? userInDb = null;
        var newUser = new User
        {
            Username = "User1",
            Email = "uusUser1@example.com",
            FirstName = "User1First",
            LastName = "User1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "PasswordHash123",
        };

        try
        {
            await DbUserCreate(newUser);

            userInDb = await DbUserGetByEmail(newUser.Email);
            Assert.That(userInDb, Is.Not.EqualTo(null));
            var userInDbPasswordHashBeforeUpdate = userInDb.PasswordHash;

            var updateUserObject = new
            {
                Username = "updatedUser1",
                Email = "uusUpdatedUser1@example.com",
                FirstName = "updatedUser1First",
                LastName = "updatedUser1Last",
                Role = UserRoleEnum.Admin,
                Password = "updatedPasswordHash123",
                ConfirmPassword = "updatedPasswordHash123",
            };
            JsonContent updateUserContent = JsonContent.Create(updateUserObject);

            var response = await Client.PutAsync($"/users/{userInDb.Id}", updateUserContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            userInDb = await DbUserGetById(userInDb.Id);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(updateUserObject.Username));
            Assert.That(userInDb.Email, Is.EqualTo(updateUserObject.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(updateUserObject.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(updateUserObject.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(updateUserObject.Role));
            Assert.That(userInDb.PasswordHash, Is.Not.EqualTo(userInDbPasswordHashBeforeUpdate));
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
    public async Task Users_Update_Succeeds_WithRandomPropertiesInRequest()
    {
        User? userInDb = null;
        var newUser = new User
        {
            Username = "User1",
            Email = "uusUser1@example.com",
            FirstName = "User1First",
            LastName = "User1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "PasswordHash123",
        };

        try
        {
            await DbUserCreate(newUser);

            userInDb = await DbUserGetByEmail(newUser.Email);
            Assert.That(userInDb, Is.Not.EqualTo(null));
            var userInDbPasswordHashBeforeUpdate = userInDb.PasswordHash;

            var updateUserObject = new
            {
                Username = "updatedUser1",
                Email = "uusUpdatedUser1@example.com",
                FirstName = "updatedUser1First",
                LastName = "updatedUser1Last",
                Role = UserRoleEnum.Admin,
                Password = "updatedPasswordHash123",
                ConfirmPassword = "updatedPasswordHash123",
                RandomPropertyOne = "RandomPropertyOne",
                RandomPropertyTwo = "RandomPropertyTwo",
                RandomPropertyThree = "RandomPropertyThree"
            };
            JsonContent updateUserContent = JsonContent.Create(updateUserObject);

            var response = await Client.PutAsync($"/users/{userInDb.Id}", updateUserContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            userInDb = await DbUserGetById(userInDb.Id);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(updateUserObject.Username));
            Assert.That(userInDb.Email, Is.EqualTo(updateUserObject.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(updateUserObject.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(updateUserObject.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(updateUserObject.Role));
            Assert.That(userInDb.PasswordHash, Is.Not.EqualTo(userInDbPasswordHashBeforeUpdate));
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
    public async Task Users_Update_Fails_WhenUserIdDoesNotExist()
    {
        var updateUserObject = new
        {
            Username = "updatedUser1",
            Email = "uusUpdatedUser1@example.com",
            FirstName = "updatedUser1First",
            LastName = "updatedUser1Last",
            Role = UserRoleEnum.Admin,
            Password = "updatedPasswordHash123",
            ConfirmPassword = "updatedPasswordHash123",
        };
        JsonContent updateUserContent = JsonContent.Create(updateUserObject);

        var response = await Client.PutAsync($"/users/{1232131}", updateUserContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    // Test: Update user with missing fields
    // Test: Update user with an email that exists for another user
    // Test: Update user without providing passwords
    // Test: Update user with non-matching passwords

    #endregion

    #region Users Delete Tests

    [Test]
    public async Task Users_Delete_Succeeds_WithMatchingDbValues()
    {
        User? userInDb = null;
        var newUser = new User
        {
            Username = "User1",
            Email = "udsUser1@example.com",
            FirstName = "User1First",
            LastName = "User1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "PasswordHash123",
        };

        try
        {
            await DbUserCreate(newUser);

            userInDb = await DbUserGetByEmail(newUser.Email);
            Assert.That(userInDb, Is.Not.EqualTo(null));

            var response = await Client.DeleteAsync($"/users/{userInDb.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            userInDb = await DbUserGetById(userInDb.Id);
            Assert.That(userInDb, Is.EqualTo(null));
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
    public async Task Users_Delete_Succeeds_WhenUserIdDoesNotExist()
    {
        var id = 2385942;

        var response = await Client.DeleteAsync($"/users/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    #endregion
}
