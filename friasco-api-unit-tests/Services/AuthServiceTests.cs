﻿using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using friasco_api.Helpers;
using friasco_api.Models;
using friasco_api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<ILogger<IAuthService>> _logger;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<IBCryptWrapper> _bcryptWrapperMock;
    private IAuthService _authService;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<IAuthService>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userServiceMock = new Mock<IUserService>();
        _bcryptWrapperMock = new Mock<IBCryptWrapper>();
        _authService = new AuthService(_logger.Object, _userRepositoryMock.Object, _userServiceMock.Object, _bcryptWrapperMock.Object);

        Environment.SetEnvironmentVariable("JWT_KEY", "F6MTF6jJ5I013c3tfXd0O+pw5QUsdCv/8v+v1KLTQjlw1amYAsFb9DqNvKLVpsFs");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("TOKEN_EXPIRY_TIME_HOUR", "1");
    }

    [Test]
    public async Task Login_ReturnsToken()
    {
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "hashedPassword"
        };
        var userAuthLoginRequestModel = new AuthLoginRequestModel
        {
            Email = "user1@example.com",
            Password = "password123"
        };

        _userRepositoryMock.Setup(x => x.GetByEmail(userAuthLoginRequestModel.Email)).ReturnsAsync(expectedUser);
        _bcryptWrapperMock.Setup(x => x.Verify(userAuthLoginRequestModel.Password, expectedUser.PasswordHash)).Returns(true);

        var jwtString = await _authService.Login(userAuthLoginRequestModel);

        Assert.That(jwtString, Is.Not.EqualTo(null));
        Assert.That(jwtString.Length, Is.AtLeast(10));

        _userRepositoryMock.Verify(x => x.GetByEmail(userAuthLoginRequestModel.Email), Times.Once());
        _bcryptWrapperMock.Verify(x => x.Verify(userAuthLoginRequestModel.Password, expectedUser.PasswordHash), Times.Once);
    }

    [Test]
    public async Task Login_ThrowsExceptionIfUserDoesNotExistAlready()
    {
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "hashedPassword"
        };
        var userAuthLoginRequestModel = new AuthLoginRequestModel
        {
            Email = "user1@example.com",
            Password = "password123"
        };

        _userRepositoryMock.Setup(x => x.GetByEmail(userAuthLoginRequestModel.Email)).ReturnsAsync((User)null);

        var exception = Assert.ThrowsAsync<AppException>(async () => await _authService.Login(userAuthLoginRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"Invalid login credentials supplied."));

        _userRepositoryMock.Verify(x => x.GetByEmail(userAuthLoginRequestModel.Email), Times.Once());
    }

    [Test]
    public async Task Login_ThrowsExceptionIfWrongPassword()
    {
        var userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            PasswordHash = "hashedPassword"
        };
        var userAuthLoginRequestModel = new AuthLoginRequestModel
        {
            Email = "user1@example.com",
            Password = "password123"
        };

        _userRepositoryMock.Setup(x => x.GetByEmail(userAuthLoginRequestModel.Email)).ReturnsAsync(expectedUser);
        _bcryptWrapperMock.Setup(x => x.Verify(userAuthLoginRequestModel.Password, expectedUser.PasswordHash)).Returns(false);

        var exception = Assert.ThrowsAsync<AppException>(async () => await _authService.Login(userAuthLoginRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"Invalid login credentials supplied."));

        _userRepositoryMock.Verify(x => x.GetByEmail(userAuthLoginRequestModel.Email), Times.Once());
        _bcryptWrapperMock.Verify(x => x.Verify(userAuthLoginRequestModel.Password, expectedUser.PasswordHash), Times.Once);
    }

    [Test]
    public async Task Register_ReturnsToken()
    {

        var userCreateRequestModel = new UserCreateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var expectedUser = new User
        {
            Id = 1,
            Username = userCreateRequestModel.Username,
            Email = userCreateRequestModel.Email,
            FirstName = userCreateRequestModel.FirstName,
            LastName = userCreateRequestModel.LastName,
            Role = userCreateRequestModel.Role,
            PasswordHash = "hashedPassword"
        };
        var createdUserAuthLoginRequestModel = new AuthLoginRequestModel
        {
            Email = userCreateRequestModel.Email,
            Password = userCreateRequestModel.Password
        };

        _userServiceMock.Setup(x => x.Create(userCreateRequestModel)).ReturnsAsync(1);
        _userRepositoryMock.Setup(x => x.GetByEmail(createdUserAuthLoginRequestModel.Email)).ReturnsAsync(expectedUser);
        _bcryptWrapperMock.Setup(x => x.Verify(createdUserAuthLoginRequestModel.Password, expectedUser.PasswordHash)).Returns(true);

        var jwtString = await _authService.Register(userCreateRequestModel);

        Assert.That(jwtString, Is.Not.EqualTo(null));
        Assert.That(jwtString.Length, Is.AtLeast(10));

        _userServiceMock.Verify(x => x.Create(userCreateRequestModel), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByEmail(userCreateRequestModel.Email), Times.Once());
        _bcryptWrapperMock.Verify(x => x.Verify(userCreateRequestModel.Password, expectedUser.PasswordHash), Times.Once);
    }
}
