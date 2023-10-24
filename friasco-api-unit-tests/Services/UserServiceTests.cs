using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using friasco_api.Models;
using friasco_api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<ILogger<IUserService>> _loggerMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private IUserService _userService;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<IUserService>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_loggerMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsAllUsers()
    {
        var expectedUsers = new List<User>
        {
            new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User },
            new User { Id = 2, Username = "User2", Email = "user2@example.com", FirstName = "user2First", LastName = "user2Last", Role = UserRoleEnum.User }
        };
        _userRepositoryMock.Setup(x => x.GetAll()).ReturnsAsync(expectedUsers);

        var users = await _userService.GetAll();

        Assert.That(users, Is.EqualTo(expectedUsers));
        _userRepositoryMock.Verify(x => x.GetAll(), Times.Once());
    }

    [Test]
    public async Task GetById_ReturnsUserById()
    {
        var userId = 1;
        var expectedUser = new User { Id = userId, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _userRepositoryMock.Setup(x => x.GetById(userId)).ReturnsAsync(expectedUser);

        var user = await _userService.GetById(userId);

        Assert.That(user, Is.EqualTo(expectedUser));
        _userRepositoryMock.Verify(x => x.GetById(userId), Times.Once());
    }

    [Test]
    public async Task Create_CreatesUser_ReturnsOneRowAffected()
    {
        Assert.Fail("TODO: Basic scaffold, not complete yet...");
        var userCreateRequestModel = new UserCreateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User
        };
        var userToCreate = new User
        {
            Id = 1,
            Username = userCreateRequestModel.Username,
            Email = userCreateRequestModel.Email,
            FirstName = userCreateRequestModel.FirstName,
            LastName = userCreateRequestModel.LastName,
            Role = userCreateRequestModel.Role
        };
        _userRepositoryMock.Setup(x => x.Create(userToCreate)).ReturnsAsync(1);

        var rowsAffected = await _userService.Create(userCreateRequestModel);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _userRepositoryMock.Verify(x => x.Create(userToCreate), Times.Once());
    }

    [Test]
    public async Task Update_UpdatesUser_ReturnsOneRowAffected()
    {
        Assert.Fail("TODO: Basic scaffold, not complete yet...");
        var userUpdateRequestModel = new UserCreateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User
        };
        var userToUpdate = new User
        {
            Id = 1,
            Username = userUpdateRequestModel.Username,
            Email = userUpdateRequestModel.Email,
            FirstName = userUpdateRequestModel.FirstName,
            LastName = userUpdateRequestModel.LastName,
            Role = userUpdateRequestModel.Role
        };
        _userRepositoryMock.Setup(x => x.Update(userToUpdate)).ReturnsAsync(1);

        var rowsAffected = await _userService.Create(userUpdateRequestModel);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _userRepositoryMock.Verify(x => x.Create(userToUpdate), Times.Once());
    }

    [Test]
    public async Task Delete_DeletesUser_ReturnsOneRowAffected()
    {
        var userToDelete = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _userRepositoryMock.Setup(x => x.Delete(userToDelete.Id)).ReturnsAsync(1);

        var rowsAffected = await _userService.Delete(userToDelete.Id);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _userRepositoryMock.Verify(x => x.Delete(userToDelete.Id), Times.Once());
    }
}