using System.Data;
using friasco_api.Data;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using Moq;

namespace friasco_api_unit_tests.Data.Repositories;

[TestFixture]
public class UserRespositoryTests
{
    private Mock<IDapperWrapper> _mockDapperWrapper;
    private Mock<IDataContext> _mockDataContext;
    private IUserRepository _userRepository;

    [SetUp]
    public void SetUp()
    {
        _mockDataContext = new Mock<IDataContext>();
        _mockDapperWrapper = new Mock<IDapperWrapper>();
        _userRepository = new UserRepository(_mockDataContext.Object, _mockDapperWrapper.Object);
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
    }

    [Test]
    public async Task GetById_ReturnsUserById()
    {
        var userId = 1;
        var expectedUser = new User { Id = userId, Username = "User1", Email = "user1@example.com", FirstName = "user1First", LastName = "user1Last", Role = UserRoleEnum.User };
        _mockDapperWrapper.Setup(x => x.QueryFirstOrDefaultAsync<User>(It.IsAny<IDbConnection>(), It.IsAny<string>(), userId)).ReturnsAsync(expectedUser);

        var user = await _userRepository.GetById(userId);

        Assert.That(user, Is.EqualTo(expectedUser));
    }

    // GetByEmail
    // Create
    // Update
    // Delete
}
