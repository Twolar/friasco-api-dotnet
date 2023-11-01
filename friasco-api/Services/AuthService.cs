using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Helpers;
using friasco_api.Models;

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

        var userToken = await GenerateToken(user);

        return userToken;
    }

    private async Task<string> GenerateToken(User user)
    {
        var token = $"RandomTokenFor";

        return token;
    }
}
