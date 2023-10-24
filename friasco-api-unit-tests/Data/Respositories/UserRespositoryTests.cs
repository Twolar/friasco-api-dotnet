using System.Data;
using friasco_api.Data;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Data.Repositories;

[TestFixture]
public class UserRespositoryTests
{
    private Mock<ILogger<IUserRepository>> _loggerMock;
    private Mock<IDapperWrapper> _mockDapperWrapper;
    private Mock<IDataContext> _mockDataContext;
    private IUserRepository _userRepository;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<IUserRepository>>();
        _mockDataContext = new Mock<IDataContext>();
        _mockDapperWrapper = new Mock<IDapperWrapper>();
        _userRepository = new UserRepository(_loggerMock.Object, _mockDataContext.Object, _mockDapperWrapper.Object);
    }

    [Test]
    public async Task GetAll_ReturnsAllUsers()
    {
        var expectedUsers = new List<User>
        {
            new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User },
            new User { Id = 2, Username = "User2", Email = "user2@example.com", FirstName = "user2First", LastName = "user2Last", Role = UserRoleEnum.User }
        };
        _mockDapperWrapper.Setup(x => x.QueryAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>())).ReturnsAsync(expectedUsers);

        var users = await _userRepository.GetAll();

        Assert.That(users, Is.EqualTo(expectedUsers));
        _mockDapperWrapper.Verify(x => x.QueryAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>()), Times.Once());
    }

    [Test]
    public async Task GetById_ReturnsUserById()
    {
        var userId = 1;
        var expectedUser = new User { Id = userId, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _mockDapperWrapper.Setup(x => x.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>(), userId)).ReturnsAsync(expectedUser);

        var user = await _userRepository.GetById(userId);

        Assert.That(user, Is.EqualTo(expectedUser));
        _mockDapperWrapper.Verify(x => x.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>(), userId), Times.Once());
    }

    [Test]
    public async Task GetByEmail_ReturnsUserByEmail()
    {
        var userEmail = "user1@example.com";
        var expectedUser = new User { Id = 1, Username = "User1", Email = userEmail, FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _mockDapperWrapper.Setup(x => x.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>(), userEmail)).ReturnsAsync(expectedUser);

        var user = await _userRepository.GetByEmail(userEmail);

        Assert.That(user, Is.EqualTo(expectedUser));
        _mockDapperWrapper.Verify(x => x.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>(), userEmail), Times.Once());
    }

    [Test]
    public async Task Create_CreatesUser_ReturnsOneRowAffected()
    {
        var userToCreate = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _mockDapperWrapper.Setup(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), userToCreate)).ReturnsAsync(1);

        var rowsAffected = await _userRepository.Create(userToCreate);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _mockDapperWrapper.Verify(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), userToCreate), Times.Once());
    }

    [Test]
    public async Task Update_UpdatesUser_ReturnsOneRowAffected()
    {
        var userToUpdate = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _mockDapperWrapper.Setup(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), userToUpdate)).ReturnsAsync(1);

        var rowsAffected = await _userRepository.Update(userToUpdate);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _mockDapperWrapper.Verify(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), userToUpdate), Times.Once());
    }

    [Test]
    public async Task Delete_DeletesUser_ReturnsOneRowAffected()
    {
        var userToDelete = new User { Id = 1, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _mockDapperWrapper.Setup(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), userToDelete.Id)).ReturnsAsync(1);

        var rowsAffected = await _userRepository.Delete(userToDelete.Id);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _mockDapperWrapper.Verify(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), userToDelete.Id), Times.Once());
    }
}
