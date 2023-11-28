using System.Net;
using System.Text;
using System.Text.Json;
using friasco_api.Data.Entities;
using friasco_api.Enums;
using friasco_api.Models;

namespace friasco_api_integration_tests.Tests;

public partial class AuthorizationTests : IntegrationTestBase
{
    [Test]
    [TestCaseSource(nameof(ApiClientNames))]
    public async Task Auth_Login_Auth(string apiClientName)
    {
        var userLoginObject = new AuthLoginRequestModel
        {
            Email = "UserRole@example.com",
            Password = "Password123",
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(userLoginObject), Encoding.UTF8, "application/json");

        var url = "/Auth/Login";

        HttpResponseMessage? response;

        switch (apiClientName)
        {
            case nameof(ApiClientWithNoAuth):
                response = await ApiClientWithNoAuth.PostAsync(url, jsonContent);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                break;
            case nameof(ApiClientWithRoleUser):
                response = await ApiClientWithRoleUser.PostAsync(url, jsonContent);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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

    [Test]
    [TestCaseSource(nameof(ApiClientNames))]
    public async Task Auth_Register_Auth(string apiClientName)
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

        var url = "Auth/Register";

        HttpResponseMessage? response;

        try
        {
            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
    [TestCaseSource(nameof(ApiClientNames))]
    public async Task Auth_Refresh_Auth(string apiClientName)
    {
        var tokenRefreshRequest = new
        {
            Token = Guid.NewGuid().ToString()
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(tokenRefreshRequest), Encoding.UTF8, "application/json");

        var url = "Auth/Register";

        HttpResponseMessage? response;

        try
        {
            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleAdmin):
                    response = await ApiClientWithRoleAdmin.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleSuperAdmin):
                    response = await ApiClientWithRoleSuperAdmin.PostAsync(url, jsonContent);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                default:
                    throw new ArgumentException($"Invalid api client name: {apiClientName}");
            }
        }
        finally
        {
        }
    }

    [Test]
    [TestCaseSource(nameof(ApiClientNames))]
    public async Task Auth_Logout_Auth(string apiClientName)
    {
        var url = "Auth/Logout";

        HttpResponseMessage? response;

        try
        {
            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleAdmin):
                    response = await ApiClientWithRoleAdmin.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleSuperAdmin):
                    response = await ApiClientWithRoleSuperAdmin.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                default:
                    throw new ArgumentException($"Invalid api client name: {apiClientName}");
            }
        }
        finally
        {
        }
    }

    [Test]
    [TestCaseSource(nameof(ApiClientNames))]
    public async Task Auth_LogoutAll_Auth(string apiClientName)
    {
        var url = "Auth/LogoutAll";

        HttpResponseMessage? response;

        try
        {
            switch (apiClientName)
            {
                case nameof(ApiClientWithNoAuth):
                    response = await ApiClientWithNoAuth.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleUser):
                    response = await ApiClientWithRoleUser.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleAdmin):
                    response = await ApiClientWithRoleAdmin.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                case nameof(ApiClientWithRoleSuperAdmin):
                    response = await ApiClientWithRoleSuperAdmin.GetAsync(url);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                    break;
                default:
                    throw new ArgumentException($"Invalid api client name: {apiClientName}");
            }
        }
        finally
        {
        }
    }
}
