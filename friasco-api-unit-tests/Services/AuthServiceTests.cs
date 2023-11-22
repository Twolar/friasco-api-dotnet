using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using friasco_api.Helpers;
using friasco_api.Models;
using friasco_api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace friasco_api_unit_tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<ILogger<IAuthService>> _logger;
    private Mock<IUserService> _userServiceMock;
    private Mock<IBCryptWrapper> _bcryptWrapperMock;
    private TokenValidationParameters _tokenValidationParameters;
    private Mock<IAuthRepository> _authRepository;
    private IAuthService _authService;

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", "F6MTF6jJ5I013c3tfXd0O+pw5QUsdCv/8v+v1KLTQjlw1amYAsFb9DqNvKLVpsFs");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("JWT_EXPIRY_SECONDS", "60");
        Environment.SetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS", "1");

        var jwtSettings = new JwtSettings
        {
            Key = Environment.GetEnvironmentVariable("JWT_KEY"),
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        };
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            RoleClaimType = ClaimTypes.Role
        };

        _logger = new Mock<ILogger<IAuthService>>();
        _userServiceMock = new Mock<IUserService>();
        _bcryptWrapperMock = new Mock<IBCryptWrapper>();
        _tokenValidationParameters = tokenValidationParameters;
        _authRepository = new Mock<IAuthRepository>();
        _authService = new AuthService(
            _logger.Object,
            _userServiceMock.Object,
            _bcryptWrapperMock.Object,
            _tokenValidationParameters,
            _authRepository.Object);
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

        _userServiceMock.Setup(x => x.GetByEmail(userAuthLoginRequestModel.Email)).ReturnsAsync(expectedUser);
        _bcryptWrapperMock.Setup(x => x.Verify(userAuthLoginRequestModel.Password, expectedUser.PasswordHash)).Returns(true);

        var authResult = await _authService.Login(userAuthLoginRequestModel);

        Assert.That(authResult.Token, Is.Not.EqualTo(null));
        Assert.That(authResult.Token.Length, Is.AtLeast(10));

        _userServiceMock.Verify(x => x.GetByEmail(userAuthLoginRequestModel.Email), Times.Once());
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

        _userServiceMock.Setup(x => x.GetByEmail(userAuthLoginRequestModel.Email)).ReturnsAsync((User)null);

        var exception = Assert.ThrowsAsync<AppException>(async () => await _authService.Login(userAuthLoginRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"Invalid login credentials supplied."));

        _userServiceMock.Verify(x => x.GetByEmail(userAuthLoginRequestModel.Email), Times.Once());
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

        _userServiceMock.Setup(x => x.GetByEmail(userAuthLoginRequestModel.Email)).ReturnsAsync(expectedUser);
        _bcryptWrapperMock.Setup(x => x.Verify(userAuthLoginRequestModel.Password, expectedUser.PasswordHash)).Returns(false);

        var exception = Assert.ThrowsAsync<AppException>(async () => await _authService.Login(userAuthLoginRequestModel));
        Assert.That(exception.Message, Is.EqualTo($"Invalid login credentials supplied."));

        _userServiceMock.Verify(x => x.GetByEmail(userAuthLoginRequestModel.Email), Times.Once());
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
        _userServiceMock.Setup(x => x.GetByEmail(createdUserAuthLoginRequestModel.Email)).ReturnsAsync(expectedUser);

        var authResult = await _authService.Register(userCreateRequestModel);

        Assert.That(authResult.Token, Is.Not.EqualTo(null));
        Assert.That(authResult.Token.Length, Is.AtLeast(10));

        _userServiceMock.Verify(x => x.Create(userCreateRequestModel), Times.Once);
        _userServiceMock.Verify(x => x.GetByEmail(userCreateRequestModel.Email), Times.Once());
    }

    [Test]
    public async Task Refresh_ReturnsToken()
    {
        var user = new User
        {
            Id = 1,
            Username = "User1",
            Email = "user1@example.com",
            FirstName = "user1First",
            LastName = "user1Last",
            Role = UserRoleEnum.User,
            Guid = Guid.NewGuid()
        };

        var accessTokenId = Guid.NewGuid().ToString();
        var accessToken = await CreateJwtToken(user, accessTokenId, 500);

        var refreshTokenExpiryDays = Convert.ToInt64(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS"));
        var storedRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = accessTokenId,
            UserGuid = user.Guid,
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            IsUsed = false,
            IsValid = true
        };

        _authRepository.Setup(a => a.GetRefreshTokenByToken(It.IsAny<string>())).ReturnsAsync(storedRefreshToken);
        _userServiceMock.Setup(u => u.GetById(It.IsAny<int>())).ReturnsAsync(user);

        var authResult = await _authService.Refresh(accessToken, storedRefreshToken.Token);

        Assert.IsNotNull(authResult);
        Assert.That(authResult.Token, Is.Not.EqualTo(null));
        Assert.That(authResult.Token.Length, Is.AtLeast(10));

        _authRepository.Verify(a => a.GetRefreshTokenByToken(It.IsAny<string>()), Times.Once);
        _userServiceMock.Verify(u => u.GetById(It.IsAny<int>()), Times.Once);
    }

    // TODO: Invalid token
    // TODO: Token has not expired
    // TODO: Refresh token does not exist
    // TODO: Refresh token has expired
    // TODO: Refresh token is not valid
    // TODO: Refresh token has been used
    // TODO: Refresh token does not match JWT
    // TODO: Can't find used by ID
    // TODO: Failed to delete refresh token

    #region 

    private async Task<string> CreateJwtToken(User user, string jwtId, int tokenExpiryInMilliseconds)
    {
        var authClaims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            Expires = DateTime.UtcNow.AddMilliseconds(tokenExpiryInMilliseconds),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(authClaims)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion
}
