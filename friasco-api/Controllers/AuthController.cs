﻿using friasco_api.Models;
using friasco_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace friasco_api.Controllers;

// TODO: Look at sending refresh token back as a HttpOnly cookie?
// - Set AllowOrigins in CORs
// - Maybe SameSite in the future too? (if on same domain?)

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

        return Ok(authResult);
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Register(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Register");

        var authResult = await _authService.Register(model);

        return Ok(authResult);
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Refresh(AuthRefreshRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "AuthController::Refresh");

        var authResult = await _authService.Refresh(model);

        return Ok(authResult);
    }
}
