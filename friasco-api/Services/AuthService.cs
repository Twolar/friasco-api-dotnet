using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using friasco_api.Data.Repositories;
using friasco_api.Helpers;
using friasco_api.Models;
using Microsoft.IdentityModel.Tokens;

namespace friasco_api.Services;

public interface IAuthService
{
    Task<string> Login(AuthLoginRequestModel model);
    Task<string> Register(UserCreateRequestModel model);
}

public class AuthService : IAuthService
{
    private readonly ILogger<IAuthService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly IBCryptWrapper _bcryptWrapper;

    public AuthService(ILogger<IAuthService> logger, IUserRepository userRepository, IUserService userService, IBCryptWrapper bcryptWrapper)
    {
        _logger = logger;
        _userRepository = userRepository;
        _userService = userService;
        _bcryptWrapper = bcryptWrapper;
    }

    public async Task<string> Login(AuthLoginRequestModel model)
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

        var authClaims = new List<Claim>() {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userToken = GenerateToken(authClaims);

        return userToken;
    }

    public async Task<string> Register(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Register");

        await _userService.Create(model);

        var newUserAuthLoginRequestModel = new AuthLoginRequestModel
        {
            Email = model.Email,
            Password = model.Password
        };

        var userJwtToken = await Login(newUserAuthLoginRequestModel);

        return userJwtToken;
    }

    private string GenerateToken(IEnumerable<Claim> claims)
    {
        _logger.Log(LogLevel.Debug, "AuthService::GenerateToken");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
        var tokenExpiryAsHours = Convert.ToInt64(Environment.GetEnvironmentVariable("TOKEN_EXPIRY_TIME_HOUR"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            Expires = DateTime.UtcNow.AddHours(tokenExpiryAsHours),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(claims)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
