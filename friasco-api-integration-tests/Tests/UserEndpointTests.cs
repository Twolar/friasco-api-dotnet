using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using friasco_api.Data.Entities;
using friasco_api.Enums;

namespace friasco_api_integration_tests.Tests;

[TestFixture]
public class UserEndpointTests : IntegrationTestBase
{
    #region Users Get Tests

    [Test]
    public async Task Users_GetAll_Succeeds_WithMatchingDbValues()
    {
        var createdDbUserList = new List<User>() {
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
            await DbUserCreate(createdDbUserList[0]);
            await DbUserCreate(createdDbUserList[1]);

            var response = await ApiClientWithRoleSuperAdmin.GetAsync("/users");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var contentJsonString = await response.Content.ReadAsStringAsync();
            List<User> apiUsers = JsonSerializer.Deserialize<List<User>>(contentJsonString, DefaultTestingJsonSerializerOptions);

            var createdUsers = apiUsers.Where(u => createdDbUserList.Any(dbu => dbu.Email == u.Email)).ToList();

            Assert.That(createdUsers.Count, Is.EqualTo(createdDbUserList.Count));

            for (var i = 0; i < createdDbUserList.Count; i++)
            {
                Assert.That(createdUsers[i].Username, Is.EqualTo(createdDbUserList[i].Username));
                Assert.That(createdUsers[i].Email, Is.EqualTo(createdDbUserList[i].Email));
                Assert.That(createdUsers[i].FirstName, Is.EqualTo(createdDbUserList[i].FirstName));
                Assert.That(createdUsers[i].LastName, Is.EqualTo(createdDbUserList[i].LastName));
                Assert.That(createdUsers[i].Role, Is.EqualTo(createdDbUserList[i].Role));
                Assert.That(createdUsers[i].PasswordHash, Is.EqualTo(null));
            }
        }
        finally
        {
            await DbUserDeleteByEmail(createdDbUserList[1].Email);
            await DbUserDeleteByEmail(createdDbUserList[0].Email);
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

            var response = await ApiClientWithRoleAdmin.GetAsync($"/users/{userInDb.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var contentJsonString = await response.Content.ReadAsStringAsync();
            User apiUserGet = JsonSerializer.Deserialize<User>(contentJsonString, DefaultTestingJsonSerializerOptions);

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

        var response = await ApiClientWithRoleSuperAdmin.GetAsync($"/users/{id}");
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

            var response = await ApiClientWithRoleSuperAdmin.PostAsync("/users", jsonContent);
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

            var response = await ApiClientWithRoleSuperAdmin.PostAsync("/users", jsonContent);
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
    public async Task Users_Create_Fails_WithMissingRequest()
    {
        var userCreateJsonObject = new
        {
            // Empty Object
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

        var response = await ApiClientWithRoleSuperAdmin.PostAsync("/users", jsonContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    [Test]
    public async Task Users_Create_Fails_WhenEmailAlreadyExists()
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

            var userCreateJsonObject = new
            {
                Username = "User1",
                Email = userInDb.Email,
                FirstName = "User1First",
                LastName = "User1Last",
                Role = UserRoleEnum.User,
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

            var response = await ApiClientWithRoleSuperAdmin.PostAsync("/users", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
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
    public async Task Users_Create_Fails_WhenPasswordsDoNotMatch()
    {
        var userCreateJsonObject = new
        {
            Username = "User1",
            Email = "User1@example.com",
            FirstName = "User1First",
            LastName = "User1Last",
            Role = UserRoleEnum.User,
            Password = "321drowssap",
            ConfirmPassword = "Password123"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

        var response = await ApiClientWithRoleSuperAdmin.PostAsync("/users", jsonContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

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

            var response = await ApiClientWithRoleSuperAdmin.PutAsync($"/users/{userInDb.Id}", updateUserContent);
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

            var response = await ApiClientWithRoleSuperAdmin.PutAsync($"/users/{userInDb.Id}", updateUserContent);
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
    public async Task Users_Create_Succeeds_WhenPasswordsNotProvided()
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
            };
            JsonContent updateUserContent = JsonContent.Create(updateUserObject);

            var response = await ApiClientWithRoleSuperAdmin.PutAsync($"/users/{userInDb.Id}", updateUserContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            userInDb = await DbUserGetById(userInDb.Id);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(updateUserObject.Username));
            Assert.That(userInDb.Email, Is.EqualTo(updateUserObject.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(updateUserObject.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(updateUserObject.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(updateUserObject.Role));
            Assert.That(userInDb.PasswordHash, Is.EqualTo(userInDbPasswordHashBeforeUpdate));
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
    public async Task Users_Update_Succeeds_WithMissingRequest()
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

            var updateUserObject = new
            {
                // Empty Request Object
            };
            JsonContent updateUserContent = JsonContent.Create(updateUserObject);

            var response = await ApiClientWithRoleSuperAdmin.PutAsync($"/users/{userInDb.Id}", updateUserContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            userInDb = await DbUserGetById(userInDb.Id);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(newUser.Username));
            Assert.That(userInDb.Email, Is.EqualTo(newUser.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(newUser.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(newUser.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(newUser.Role));
            Assert.That(userInDb.PasswordHash, Is.EqualTo(newUser.PasswordHash));
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

        var response = await ApiClientWithRoleSuperAdmin.PutAsync($"/users/{1232131}", updateUserContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    [Test]
    public async Task Users_Update_Fails_WhenEmailAlreadyExists()
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

            var userCreateJsonObject = new
            {
                Username = "User1",
                Email = userInDb.Email,
                FirstName = "User1First",
                LastName = "User1Last",
                Role = UserRoleEnum.User,
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

            var response = await ApiClientWithRoleSuperAdmin.PostAsync("/users", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
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
    public async Task Users_Update_Fails_WhenPasswordsDoNotMatch()
    {
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
                    Password = "321Password",
                    ConfirmPassword = "updatedPasswordHash123",
                };
                JsonContent updateUserContent = JsonContent.Create(updateUserObject);

                var response = await ApiClientWithRoleSuperAdmin.PutAsync($"/users/{userInDb.Id}", updateUserContent);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));

                userInDb = await DbUserGetById(userInDb.Id);

                Assert.That(userInDb, Is.Not.EqualTo(null));
                Assert.That(userInDb.Username, Is.EqualTo(newUser.Username));
                Assert.That(userInDb.Email, Is.EqualTo(newUser.Email));
                Assert.That(userInDb.FirstName, Is.EqualTo(newUser.FirstName));
                Assert.That(userInDb.LastName, Is.EqualTo(newUser.LastName));
                Assert.That(userInDb.Role, Is.EqualTo(newUser.Role));
                Assert.That(userInDb.PasswordHash, Is.EqualTo(newUser.PasswordHash));
            }
            finally
            {
                if (userInDb != null)
                {
                    await DbUserDeleteById(userInDb.Id);

                }
            }
        }
    }

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

            var response = await ApiClientWithRoleSuperAdmin.DeleteAsync($"/users/{userInDb.Id}");
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

        var response = await ApiClientWithRoleSuperAdmin.DeleteAsync($"/users/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(false));
    }

    #endregion
}
