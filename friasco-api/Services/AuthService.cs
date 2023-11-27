using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Helpers;
using friasco_api.Models;
using Microsoft.IdentityModel.Tokens;

namespace friasco_api.Services;

public interface IAuthService
{
    Task<AuthResultModel> Login(AuthLoginRequestModel model);
    Task<AuthResultModel> Register(UserCreateRequestModel model);
    Task<AuthResultModel> Refresh(string accessTokenJti, string refreshToken);
    Task Logout(string refreshToken);
    Task LogoutAll(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly ILogger<IAuthService> _logger;
    private readonly IUserService _userService;
    private readonly IBCryptWrapper _bcryptWrapper;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IAuthRepository _authRepository;

    public AuthService(
        ILogger<IAuthService> logger,
        IUserService userService,
        IBCryptWrapper bcryptWrapper,
        TokenValidationParameters tokenValidationParameters,
        IAuthRepository authRepository)
    {
        _logger = logger;
        _userService = userService;
        _bcryptWrapper = bcryptWrapper;
        _tokenValidationParameters = tokenValidationParameters;
        _authRepository = authRepository;
    }

    public async Task<AuthResultModel> Login(AuthLoginRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Login");

        var invalidCredentialsString = "Invalid login credentials supplied.";

        var user = await _userService.GetByEmail(model.Email!);

        if (user == null)
        {
            throw new AppException(invalidCredentialsString);
        }

        bool userVerified = _bcryptWrapper.Verify(model.Password, user.PasswordHash);
        if (!userVerified)
        {
            throw new AppException(invalidCredentialsString);
        }

        var authResult = await GenerateAuthResultForUser(user);

        return authResult;
    }

    public async Task<AuthResultModel> Register(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Register");

        await _userService.Create(model);

        var user = await _userService.GetByEmail(model.Email!);

        var authResult = await GenerateAuthResultForUser(user);

        return authResult;
    }

    public async Task<AuthResultModel> Refresh(string accessTokenJti, string refreshToken)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Refresh");

        var storedRefreshToken = await _authRepository.GetRefreshTokenByToken(refreshToken);
        if (storedRefreshToken == null)
        {
            throw new AppException("Refresh token does not exist"); // TODO: Testing, Change so user does not have too much info
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpirationDate)
        {
            throw new AppException("Refresh token has expired"); // TODO: Testing, Change so user does not have too much info
        }

        if (!storedRefreshToken.IsValid)
        {
            throw new AppException("Refresh token is not valid"); // TODO: Testing, Change so user does not have too much info
        }

        if (storedRefreshToken.IsUsed)
        {
            throw new AppException("Refresh token has been used"); // TODO: Testing, Change so user does not have too much info
        }

        if (storedRefreshToken.JwtId != accessTokenJti)
        {
            throw new AppException("Refresh token does not match JWT"); // TODO: Testing, Change so user does not have too much info
        }

        var user = await _userService.GetByGuid(storedRefreshToken!.UserGuid!.Value);

        // Delete old refresh token as it is about to be rotated to a newly generated one
        await _authRepository.DeleteRefreshTokenByJwtId(storedRefreshToken.JwtId);

        return await GenerateAuthResultForUser(user);
    }

    public async Task Logout(string refreshToken)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Logout");

        var storedRefreshToken = await _authRepository.GetRefreshTokenByToken(refreshToken);

        if (storedRefreshToken == null)
        {
            throw new AppException("Refresh token does not exist"); // TODO: Testing, Change so user does not have too much info
        }

        await _authRepository.DeleteRefreshTokenByJwtId(storedRefreshToken!.JwtId);
    }

    public async Task LogoutAll(string refreshToken)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Logout");

        var storedRefreshToken = await _authRepository.GetRefreshTokenByToken(refreshToken);

        if (storedRefreshToken == null)
        {
            throw new AppException("Refresh token does not exist"); // TODO: Testing, Change so user does not have too much info
        }

        var userGuid = storedRefreshToken!.UserGuid!.Value;

        await _authRepository.DeleteRefreshTokensByUserGuid(userGuid);
    }

    #region Helpers

    private async Task<AuthResultModel> GenerateAuthResultForUser(User user)
    {
        _logger.Log(LogLevel.Debug, "AuthService::GenerateToken");

        var authClaims = new List<Claim>() {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
        var jwtTokenExpirySeconds = Convert.ToInt64(Environment.GetEnvironmentVariable("JWT_EXPIRY_SECONDS"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            Expires = DateTime.UtcNow.AddSeconds(jwtTokenExpirySeconds),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(authClaims)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        var refreshTokenExpiryDays = Convert.ToInt64(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS"));
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            JwtId = token.Id,
            UserGuid = user.Guid,
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
            // IsUsed defaulted to 0 in DB
            // IsValid defaulted to 1 in DB
        };

        // Add refresh token to the DB
        await _authRepository.CreateRefreshToken(refreshToken);

        return new AuthResultModel(tokenHandler.WriteToken(token), token.Id, refreshToken.Token);
    }

    #endregion
}
