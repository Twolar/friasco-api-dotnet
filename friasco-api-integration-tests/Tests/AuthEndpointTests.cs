using System.Net;
using System.Text;
using System.Text.Json;
using friasco_api.Data.Entities;
using friasco_api.Enums;
using friasco_api_integration_tests.Helpers;

namespace friasco_api_integration_tests.Tests;

public class AuthEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task Auth_Login_ReturnsOkResult_WithToken()
    {
        var apiUserLoginObject = new
        {
            Email = "UserRole@example.com",
            Password = "Password123",
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(apiUserLoginObject), Encoding.UTF8, "application/json");

        var response = await ApiClientWithRoleSuperAdmin.PostAsync("/login", jsonContent);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

        var contentJsonString = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(contentJsonString, DefaultTestingJsonSerializerOptions);
        Assert.That(loginResponse.Token, Is.Not.EqualTo(string.Empty));
        Assert.That(loginResponse.Token, Is.Not.EqualTo(null));
    }

    [Test]
    public async Task Auth_Register_ReturnsOkResult_WithToken()
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

            var response = await ApiClientWithRoleSuperAdmin.PostAsync("/register", jsonContent);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));

            var contentJsonString = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(contentJsonString, DefaultTestingJsonSerializerOptions);
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
}
