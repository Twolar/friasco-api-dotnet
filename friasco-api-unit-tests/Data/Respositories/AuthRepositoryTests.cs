using System.Data;
using friasco_api.Data;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace friasco_api_unit_tests.Data.Repositories;

[TestFixture]
public class AuthRepositoryTests
{
    private Mock<ILogger<IAuthRepository>> _loggerMock;
    private Mock<IDapperWrapper> _mockDapperWrapper;
    private Mock<IDataContext> _mockDataContext;
    private IAuthRepository _authRepository;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<IAuthRepository>>();
        _mockDapperWrapper = new Mock<IDapperWrapper>();
        _mockDataContext = new Mock<IDataContext>();
        _authRepository = new AuthRepository(
            _loggerMock.Object,
            _mockDataContext.Object,
            _mockDapperWrapper.Object);
    }

    [Test]
    public async Task GetRefreshTokenByToken_ReturnsToken()
    {
        var expectedToken = new RefreshToken
        {
            Id = 1,
            UserGuid = Guid.NewGuid(),
            JwtId = "RandomGuidId",
            Token = "TokenItself",
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
            IsValid = true
        };
        _mockDapperWrapper.Setup(x => x.QueryFirstOrDefaultAsync<RefreshToken>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(expectedToken);

        var refreshToken = await _authRepository.GetRefreshTokenByToken(expectedToken.Token);

        Assert.That(refreshToken, Is.EqualTo(expectedToken));
        _mockDapperWrapper.Verify(x => x.QueryFirstOrDefaultAsync<RefreshToken>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once());
    }

    // CreateRefreshToken(RefreshToken refreshToken);
    [Test]
    public async Task CreateRefreshToken_ReturnsOneRowAffected()
    {
        var tokenToCreate = new RefreshToken
        {
            Id = 1,
            UserGuid = Guid.NewGuid(),
            JwtId = "RandomGuidId",
            Token = "TokenItself",
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
            IsValid = true
        };
        _mockDapperWrapper.Setup(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), tokenToCreate)).ReturnsAsync(1);

        var rowsAffected = await _authRepository.CreateRefreshToken(tokenToCreate);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _mockDapperWrapper.Verify(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), tokenToCreate), Times.Once());
    }

    [Test]
    public async Task UpdateRefreshToken_ReturnsOneRowAffected()
    {
        var tokenToUpdate = new RefreshToken
        {
            Id = 1,
            UserGuid = Guid.NewGuid(),
            JwtId = "RandomGuidId",
            Token = "TokenItself",
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
            IsValid = true
        };
        _mockDapperWrapper.Setup(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), tokenToUpdate)).ReturnsAsync(1);

        var rowsAffected = await _authRepository.UpdateRefreshToken(tokenToUpdate);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _mockDapperWrapper.Verify(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), tokenToUpdate), Times.Once());
    }

    [Test]
    public async Task DeleteRefreshTokenByJwtId_ReturnsOneRowAffected()
    {
        var tokenToDelete = new RefreshToken
        {
            Id = 1,
            UserGuid = Guid.NewGuid(),
            JwtId = "RandomGuidId",
            Token = "TokenItself",
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
            IsValid = true
        };
        var objectParams = new { id = tokenToDelete.Id };
        _mockDapperWrapper.Setup(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(1);

        var rowsAffected = await _authRepository.DeleteRefreshTokenByJwtId(tokenToDelete.JwtId);

        Assert.That(rowsAffected, Is.EqualTo(1));
        _mockDapperWrapper.Verify(x => x.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once());
    }
}
