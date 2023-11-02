using friasco_api.Models;
using friasco_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace friasco_api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpPost]
    [Route("/login")]
    public async Task<IActionResult> Login(AuthLoginRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Login");

        var userJwtToken = await _authService.Login(model);

        return Ok(new { token = userJwtToken });
    }

    [HttpPost]
    [Route("/register")]
    public async Task<IActionResult> Register(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Register");

        var userJwtToken = await _authService.Register(model);

        return Ok(
            new
            {
                message = "User Created",
                token = userJwtToken
            }
        );
    }
}
