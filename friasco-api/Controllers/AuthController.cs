using friasco_api.Helpers;
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
    [Route("[action]")]
    public async Task<IActionResult> Login(AuthLoginRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Login");

        var authResult = await _authService.Login(model);

        AddAuthCookieToResponse(authResult);

        return Ok(
            new
            {
                token = authResult.Token,
            }
        );
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Register(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Register");

        var authResult = await _authService.Register(model);

        AddAuthCookieToResponse(authResult);

        return Ok(
            new
            {
                token = authResult.Token,
            }
        );
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Refresh(AuthTokenRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Refresh");

        if (!HttpContext.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken))
        {
            throw new AppException("No refresh token in auth cookie"); // TODO: Testing, Change so user does not have too much info
        }

        var authResult = await _authService.Refresh(model.Token!, refreshToken);

        AddAuthCookieToResponse(authResult);

        return Ok(
            new
            {
                token = authResult.Token,
            }
        );
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Logout(AuthTokenRequestModel model)
    {
        return Ok();
    }

    private void AddAuthCookieToResponse(AuthResultModel authResult)
    {
        // Add cookie with new RefreshToken in the response
        HttpContext.Response.Cookies.Append("X-Refresh-Token", authResult.RefreshToken, new CookieOptions()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true // This is required when SameSite is None

            //SameSite = SameSiteMode.Strict // TODO: Production, Activate before deploying
        });
    }
}
