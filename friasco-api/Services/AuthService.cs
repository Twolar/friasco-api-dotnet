using friasco_api.Models;

namespace friasco_api.Services;

public interface IAuthService
{
    Task<string> Login(AuthLoginRequestModel model);
}

public class AuthService : IAuthService
{
    private readonly ILogger<IAuthService> _logger;

    public AuthService(ILogger<IAuthService> logger)
    {
        _logger = logger;
    }

    public async Task<string> Login(AuthLoginRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthService::Login");

        // Verify Email & Password

        var token = await GenerateToken();

        return token;
    }

    private async Task<string> GenerateToken()
    {
        var token = "RandomToken";

        return token;
    }
}
