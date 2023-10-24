using friasco_api.Models;
using friasco_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace friasco_api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;

    public UsersController(ILogger<UsersController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogDebug("UsersController::GetAll");
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogDebug($"UsersController::GetById id: {id}");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "UsersController::Create");
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateRequestModel model)
    {
        _logger.LogDebug($"UsersController::Update id: {id}");
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogDebug($"UsersController::Delete id: {id}");
        return Ok();
    }
}
