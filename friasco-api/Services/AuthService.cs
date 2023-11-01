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
}

public class AuthService : IAuthService
{
    private readonly ILogger<IAuthService> _logger;
    private readonly IUserRepository _userRepository;

    public AuthService(ILogger<IAuthService> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<string> Login(AuthLoginRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Login");

        var invalidCredentialsString = "Invalid login credentials supplied.";

        var user = await _userRepository.GetByEmail(model.Email!);

        if (user == null)
        {
            // TODO: test this outcome
            throw new AppException(invalidCredentialsString);
        }

        // TODO: test this outcome
        bool userVerified = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
        if (!userVerified)
        {
            // TODO: test this outcome
            throw new AppException(invalidCredentialsString);
        }

        var authClaims = new List<Claim>() {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userToken = await GenerateToken(authClaims);

        return userToken;
    }

    private async Task<string> GenerateToken(IEnumerable<Claim> claims)
    {
        // TODO: Test me...

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
        var tokenExpiryAsHours = Convert.ToInt64(Environment.GetEnvironmentVariable("TOKEN_EXPIRY_TIME_HOUR"));
        var tokenDescriptor = new SecurityTokenDescriptor {
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"), // change
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"), // Change 
            Expires = DateTime.UtcNow.AddHours(tokenExpiryAsHours),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(claims)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
