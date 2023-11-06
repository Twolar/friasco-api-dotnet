using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using friasco_api.Data.Entities;
using friasco_api.Enums;

namespace friasco_api_integration_tests.Tests;

public partial class AuthEndpointTests : IntegrationTestBase
{
    private static List<string> _apiClientNames = new List<string> {
        nameof(ApiClientWithNoAuth),
        nameof(ApiClientWithRoleUser),
        nameof(ApiClientWithRoleAdmin),
        nameof(ApiClientWithRoleSuperAdmin)
    };

    [Test]
    [TestCaseSource(nameof(_apiClientNames))]
    public async Task Users_GetAll_Auth(string apiClientName)
    {
        var url = "/users";

        HttpResponseMessage? response;

        switch (apiClientName)
        {
            case nameof(ApiClientWithNoAuth):
                response = await ApiClientWithNoAuth.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                break;
            case nameof(ApiClientWithRoleUser):
                response = await ApiClientWithRoleUser.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                break;
            case nameof(ApiClientWithRoleAdmin):
                response = await ApiClientWithRoleAdmin.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                break;
            case nameof(ApiClientWithRoleSuperAdmin):
                response = await ApiClientWithRoleSuperAdmin.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                break;
            default:
                throw new ArgumentException($"Invalid api client name: {apiClientName}");
        }
    }

    [Test]
    [TestCaseSource(nameof(_apiClientNames))]
    public async Task Users_Get_Auth(string apiClientName)
    {
        var url = $"/users/{85732}";

        HttpResponseMessage? response;

        switch (apiClientName)
        {
            case nameof(ApiClientWithNoAuth):
                response = await ApiClientWithNoAuth.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                break;
            case nameof(ApiClientWithRoleUser):
                response = await ApiClientWithRoleUser.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                break;
            case nameof(ApiClientWithRoleAdmin):
                response = await ApiClientWithRoleAdmin.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                break;
            case nameof(ApiClientWithRoleSuperAdmin):
                response = await ApiClientWithRoleSuperAdmin.GetAsync(url);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                break;
            default:
                throw new ArgumentException($"Invalid api client name: {apiClientName}");
        }
    }

    [Test]
    [TestCaseSource(nameof(_apiClientNames))]
    public async Task Users_Create_Auth(string apiClientName)
    {
        User? userInDb = null;

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

        var url = "/users";

        HttpResponseMessage? response;

        try
        {
            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                    break;
                case nameof(ApiClientWithRoleAdmin):
                    response = await ApiClientWithRoleAdmin.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                case nameof(ApiClientWithRoleSuperAdmin):
                    response = await ApiClientWithRoleSuperAdmin.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                default:
                    throw new ArgumentException($"Invalid api client name: {apiClientName}");
            }
        }
        finally
        {
            userInDb = await DbUserGetByEmail(userCreateJsonObject.Email);
            if (userInDb != null)
            {
                await DbUserDeleteById(userInDb.Id);
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(_apiClientNames))]
    public async Task Users_Update_Auth(string apiClientName)
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

        var updateUserObject = new
        {
        };
        JsonContent updateUserContent = JsonContent.Create(updateUserObject);

        HttpResponseMessage? response;

        try
        {
            await DbUserCreate(newUser);

            userInDb = await DbUserGetByEmail(newUser.Email);
            Assert.That(userInDb, Is.Not.EqualTo(null));

            var url = $"/users/{userInDb.Id}";

            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.PutAsync(url, updateUserContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.PutAsync(url, updateUserContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                    break;
                case nameof(ApiClientWithRoleAdmin):
                    response = await ApiClientWithRoleAdmin.PutAsync(url, updateUserContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                case nameof(ApiClientWithRoleSuperAdmin):
                    response = await ApiClientWithRoleSuperAdmin.PutAsync(url, updateUserContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                default:
                    throw new ArgumentException($"Invalid api client name: {apiClientName}");
            }
        }
        finally
        {
            if (userInDb != null)
            {
                await DbUserDeleteByEmail(newUser.Email);
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(_apiClientNames))]
    public async Task Users_Delete_Auth(string apiClientName)
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

        HttpResponseMessage? response;

        try
        {
            await DbUserCreate(newUser);

            userInDb = await DbUserGetByEmail(newUser.Email);
            Assert.That(userInDb, Is.Not.EqualTo(null));

            var url = $"/users/{userInDb.Id}";

            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.DeleteAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.DeleteAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                    break;
                case nameof(ApiClientWithRoleAdmin):
                    response = await ApiClientWithRoleAdmin.DeleteAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                case nameof(ApiClientWithRoleSuperAdmin):
                    response = await ApiClientWithRoleSuperAdmin.DeleteAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                default:
                    throw new ArgumentException($"Invalid api client name: {apiClientName}");
            }
        }
        finally
        {
            if (userInDb != null)
            {
                await DbUserDeleteByEmail(newUser.Email);
            }
        }
    }

    // Leaving here for now decide where to put them, need to test:
    // Login
    // Register (CreateUser)
    // Tiered based role auth, i.e
    //      If user is superadmin then should have access to everything below this
    //      If user is admin should have admin and below, but not superadmin
}
