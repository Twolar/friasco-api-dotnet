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
    Task<AuthResponseModel> Login(AuthLoginRequestModel model);
    Task<AuthResponseModel> Refresh(AuthRefreshRequestModel model);
    Task<AuthResponseModel> Register(UserCreateRequestModel model);
}

public class AuthService : IAuthService
{
    private readonly ILogger<IAuthService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly IBCryptWrapper _bcryptWrapper;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IAuthRepository _authRepository;

    public AuthService(
        ILogger<IAuthService> logger,
        IUserRepository userRepository,
        IUserService userService,
        IBCryptWrapper bcryptWrapper,
        TokenValidationParameters tokenValidationParameters,
        IAuthRepository authRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _userService = userService;
        _bcryptWrapper = bcryptWrapper;
        _tokenValidationParameters = tokenValidationParameters;
        _authRepository = authRepository;
    }

    public async Task<AuthResponseModel> Login(AuthLoginRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Login");

        var invalidCredentialsString = "Invalid login credentials supplied.";

        var user = await _userRepository.GetByEmail(model.Email!);

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

    public async Task<AuthResponseModel> Register(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Register");

        await _userService.Create(model);

        var user = await _userRepository.GetByEmail(model.Email);

        var authResult = await GenerateAuthResultForUser(user);

        return authResult;
    }

    public async Task<AuthResponseModel> Refresh(AuthRefreshRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Refresh");

        var tokenClaimsPrinciple = GetClaimsPrincipalFromToken(model.Token);
        if (tokenClaimsPrinciple == null)
        {
            throw new AppException("Invalid token");
        }

        var jwtExp = tokenClaimsPrinciple.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value;
        DateTimeOffset expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtExp));
        DateTime tokenExpirationDateTime = expirationTime.DateTime;
        if (tokenExpirationDateTime > DateTime.UtcNow)
        {
            throw new AppException("Token has not expired"); // TODO: Change so user does not have too much info
        }

        var jti = tokenClaimsPrinciple.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var storedRefreshToken = await _authRepository.GetRefreshTokenByJwtId(jti);
        if (storedRefreshToken == null)
        {
            throw new AppException("Token does not exist"); // TODO: Change so user does not have too much info
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpirationDate)
        {
            throw new AppException("Refresh token has expired"); // TODO: Change so user does not have too much info
        }

        if (!storedRefreshToken.IsValid)
        {
            throw new AppException("Refresh token is not valid"); // TODO: Change so user does not have too much info
        }

        if (storedRefreshToken.IsUsed)
        {
            throw new AppException("Refresh token has been used"); // TODO: Change so user does not have too much info
        }

        if (storedRefreshToken.JwtId != jti)
        {
            throw new AppException("Refresh token does not match JWT"); // TODO: Change so user does not have too much info
        }

        await _authRepository.DeleteRefreshTokenByJwtId(storedRefreshToken.JwtId);

        var userId = Convert.ToInt32(tokenClaimsPrinciple.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        var user = await _userService.GetById(userId);

        return await GenerateAuthResultForUser(user);
    }

    private ClaimsPrincipal? GetClaimsPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principle = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }
            return principle;
        }
        catch
        {
            return null;
        }
    }

    private bool IsJwtWithValidSecurityAlgorithm(SecurityToken securityToken)
    {
        if (securityToken is JwtSecurityToken jwtSecurityToken)
        {
            if (jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<AuthResponseModel> GenerateAuthResultForUser(User user)
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
        };

        // Check for old refresh tokens with JwtId & Delete or set them to invalid...

        await _authRepository.CreateRefreshToken(refreshToken);

        return new AuthResponseModel
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken.Token
        };
    }
}
