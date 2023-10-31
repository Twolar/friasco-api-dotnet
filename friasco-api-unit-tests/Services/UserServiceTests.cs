using AutoMapper;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using friasco_api.Helpers;
using friasco_api.Models;
using friasco_api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<ILogger<IUserService>> _loggerMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private IUserService _userService;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<IUserService>>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _userService = new UserService(_loggerMock.Object, _mapperMock.Object, _userRepositoryMock.Object);
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
    public async Task GetById_ThrowsExceptionIfUserDoesNotExistAlready()
    {
        var expectedUser = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _userRepositoryMock.Setup(x => x.GetById(expectedUser.Id)).ReturnsAsync((User)null);

        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _userService.GetById(expectedUser.Id));
        Assert.That(exception.Message, Is.EqualTo($"User with id: {expectedUser.Id} not found"));

        _userRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
    }

    [Test]
    public async Task Create_CreatesUser_ReturnsOneRowAffected()
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
        var userToCreate = new User
        {
            Id = 1,
            Username = userCreateRequestModel.Username,
            Email = userCreateRequestModel.Email,
            FirstName = userCreateRequestModel.FirstName,
            LastName = userCreateRequestModel.LastName,
            Role = userCreateRequestModel.Role,
        };

        _userRepositoryMock.Setup(x => x.GetByEmail(userCreateRequestModel.Email)).ReturnsAsync((User)null);
        _userRepositoryMock.Setup(x => x.Create(userToCreate)).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<User>(userCreateRequestModel)).Returns(userToCreate);

        var rowsAffected = await _userService.Create(userCreateRequestModel);

        Assert.That(rowsAffected, Is.EqualTo(1));

        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>()), Times.Once());
        _mapperMock.Verify(x => x.Map<User>(It.IsAny<UserCreateRequestModel>()), Times.Once);
        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once());
    }

    [Test]
    public async Task Create_ThrowsExceptionIfDuplicateEmailExists()
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
        var existingUser = new User
        {
            Id = 1,
            Email = userCreateRequestModel.Email,
        };

        _userRepositoryMock.Setup(x => x.GetByEmail(userCreateRequestModel.Email)).ReturnsAsync(existingUser);

        var exception = Assert.ThrowsAsync<CustomAppException>(async () => await _userService.Create(userCreateRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"User with the email: {userCreateRequestModel.Email} already exists"));

        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public async Task Update_UpdatesUser_ReturnsOneRowAffected()
    {
        var userUpdateRequestModel = new UserUpdateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Password = "password123",
            ConfirmPassword = "password123"
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

        _userRepositoryMock.Setup(x => x.GetById(userToUpdate.Id)).ReturnsAsync(userToUpdate);
        _userRepositoryMock.Setup(x => x.Update(userToUpdate)).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map(userUpdateRequestModel, userToUpdate)).Returns(userToUpdate);

        var rowsAffected = await _userService.Update(userToUpdate.Id, userUpdateRequestModel);

        Assert.That(rowsAffected, Is.EqualTo(1));

        _userRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once());
    }

    [Test]
    public async Task Update_ThrowsExceptionUserModelEmailAlreadyExists()
    {
        var userUpdateRequestModel = new UserUpdateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var userToUpdate = new User
        {
            Id = 1,
            Username = userUpdateRequestModel.Username,
            Email = "user2@example.com",
            FirstName = userUpdateRequestModel.FirstName,
            LastName = userUpdateRequestModel.LastName,
            Role = userUpdateRequestModel.Role
        };
        var userExistingWithSameEmail = new User
        {
            Id = 2,
            Email = userUpdateRequestModel.Email,
        };

        _userRepositoryMock.Setup(x => x.GetById(userToUpdate.Id)).ReturnsAsync(userToUpdate);
        _userRepositoryMock.Setup(x => x.GetByEmail(userUpdateRequestModel.Email)).ReturnsAsync(userExistingWithSameEmail);

        var exception = Assert.ThrowsAsync<CustomAppException>(async () => await _userService.Update(userToUpdate.Id, userUpdateRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"User with the email: {userUpdateRequestModel.Email} already exists"));

        _userRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public async Task Update_ThrowsExceptionIfUserDoesNotExistAlready()
    {
        var userUpdateRequestModel = new UserUpdateRequestModel
        {
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var userToUpdate = new User
        {
            Id = 1,
        };

        _userRepositoryMock.Setup(x => x.GetById(userToUpdate.Id)).ReturnsAsync((User)null);

        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _userService.Update(userToUpdate.Id, userUpdateRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"User with id: {userToUpdate.Id} not found"));

        _userRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
    }


    [Test]
    public async Task Delete_DeletesUser_ReturnsOneRowAffected()
    {
        var userToDelete = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _userRepositoryMock.Setup(x => x.GetById(userToDelete.Id)).ReturnsAsync(userToDelete);
        _userRepositoryMock.Setup(x => x.Delete(userToDelete.Id)).ReturnsAsync(1);

        var rowsAffected = await _userService.Delete(userToDelete.Id);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _userRepositoryMock.Verify(x => x.Delete(userToDelete.Id), Times.Once());
    }

    [Test]
    public async Task Delete_ThrowsExceptionIfUserDoesNotExistAlready()
    {
        var userToDelete = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _userRepositoryMock.Setup(x => x.GetById(userToDelete.Id)).ReturnsAsync((User)null);

        var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _userService.Delete(userToDelete.Id));
        Assert.That(exception.Message, Is.EqualTo($"User with id: {userToDelete.Id} not found"));

        _userRepositoryMock.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
    }
}