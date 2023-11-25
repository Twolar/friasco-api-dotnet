using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using friasco_api;
using friasco_api.Data.Entities;
using friasco_api.Enums;
using friasco_api.Models;
using friasco_api_integration_tests.Helpers;

namespace friasco_api_integration_tests.Tests;

public class AuthEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task Auth_Login_Succeeds_WithTokenReturned()
    {
        var apiUserLoginObject = new
        {
            Email = "UserRole@example.com",
            Password = "Password123",
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(apiUserLoginObject), Encoding.UTF8, "application/json");

        var response = await ApiClientWithRoleSuperAdmin.PostAsync("/Auth/Login", jsonContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

        var contentJsonString = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<AuthResponse>(contentJsonString, DefaultTestingJsonSerializerOptions);
        Assert.That(loginResponse.Token, Is.Not.EqualTo(string.Empty));
        Assert.That(loginResponse.Token, Is.Not.EqualTo(null));
    }

    [Test]
    public async Task Auth_Register_Succeeds_WithTokenReturned()
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

            var response = await ApiClientWithRoleSuperAdmin.PostAsync("/Auth/Register", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var contentJsonString = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<AuthResponse>(contentJsonString, DefaultTestingJsonSerializerOptions);
            Assert.That(registerResponse.Token, Is.Not.EqualTo(string.Empty));
            Assert.That(registerResponse.Token, Is.Not.EqualTo(null));

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
    public async Task Auth_Register_Succeeds_WithUserRoleDefaulted_WhenApiClientNotSuperAdmin()
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
                Role = UserRoleEnum.SuperAdmin,
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

            var response = await ApiClientWithRoleAdmin.PostAsync("/Auth/Register", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var contentJsonString = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<AuthResponse>(contentJsonString, DefaultTestingJsonSerializerOptions);
            Assert.That(registerResponse.Token, Is.Not.EqualTo(string.Empty));
            Assert.That(registerResponse.Token, Is.Not.EqualTo(null));

            userInDb = await DbUserGetByEmail(userCreateJsonObject.Email);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(userCreateJsonObject.Username));
            Assert.That(userInDb.Email, Is.EqualTo(userCreateJsonObject.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(userCreateJsonObject.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(userCreateJsonObject.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(UserRoleEnum.User));
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
    public async Task Auth_Register_Succeeds_WithUserRoleDefaulted_WhenApiClientIsUnauthenticated()
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
                Role = UserRoleEnum.Admin,
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(userCreateJsonObject), Encoding.UTF8, "application/json");

            var response = await ApiClientWithNoAuth.PostAsync("/Auth/Register", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var contentJsonString = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<AuthResponse>(contentJsonString, DefaultTestingJsonSerializerOptions);
            Assert.That(registerResponse.Token, Is.Not.EqualTo(string.Empty));
            Assert.That(registerResponse.Token, Is.Not.EqualTo(null));

            userInDb = await DbUserGetByEmail(userCreateJsonObject.Email);

            Assert.That(userInDb, Is.Not.EqualTo(null));
            Assert.That(userInDb.Username, Is.EqualTo(userCreateJsonObject.Username));
            Assert.That(userInDb.Email, Is.EqualTo(userCreateJsonObject.Email));
            Assert.That(userInDb.FirstName, Is.EqualTo(userCreateJsonObject.FirstName));
            Assert.That(userInDb.LastName, Is.EqualTo(userCreateJsonObject.LastName));
            Assert.That(userInDb.Role, Is.EqualTo(UserRoleEnum.User));
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
    public async Task Auth_Refresh_Succeeds_WithNewTokens()
    {
        var originalJwtExpiry = Environment.GetEnvironmentVariable("JWT_EXPIRY_SECONDS");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_SECONDS", "1");

        try
        {
            var client = Factory.CreateClient();

            var apiUserLoginObject = new
            {
                Email = "SuperAdminRole@example.com",
                Password = "Password123",
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(apiUserLoginObject), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/Auth/Login", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            Assert.That(response.Headers.Contains("Set-Cookie"));
            Assert.That(response.Headers.GetValues("Set-Cookie").Any(h => h.Contains("X-Refresh-Token")));

            var refreshToken = response.Headers.GetValues("Set-Cookie")
                .First(h => h.StartsWith("X-Refresh-Token"))
                .Split(';')[0]
                .Split('=')[1];
            client.DefaultRequestHeaders.Add("Cookie", $"X-Refresh-Token={refreshToken}");

            var contentJsonString = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<AuthResultModel>(contentJsonString, DefaultTestingJsonSerializerOptions);
            Assert.That(loginResponse.Token, Is.Not.EqualTo(string.Empty));
            Assert.That(loginResponse.Token, Is.Not.EqualTo(null));

            // Sleep for 1 seconds so that jwt expires
            await Task.Delay(1000);

            var apiRefreshObject = new
            {
                token = loginResponse.Token
            };
            jsonContent = new StringContent(JsonSerializer.Serialize(apiRefreshObject), Encoding.UTF8, "application/json");

            response = await client.PostAsync("/Auth/Refresh", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            contentJsonString = await response.Content.ReadAsStringAsync();
            var refreshResponse = JsonSerializer.Deserialize<AuthResultModel>(contentJsonString, DefaultTestingJsonSerializerOptions);
            Assert.That(refreshResponse.Token, Is.Not.EqualTo(string.Empty));
            Assert.That(refreshResponse.Token, Is.Not.EqualTo(null));

            // Assert new JWT is created via using HttpOnly Cookie RefreshToken
            Assert.That(refreshResponse.Token, Is.Not.EqualTo(loginResponse.Token));
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_EXPIRY_SECONDS", originalJwtExpiry);
        }
    }
}
