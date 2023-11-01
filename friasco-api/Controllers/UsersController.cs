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

        var users = await _userService.GetAll();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogDebug($"UsersController::GetById id: {id}");

        var user = await _userService.GetById(id);

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "UsersController::Create");

        await _userService.Create(model);

        return Ok(
            new { message = "User Created" }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateRequestModel model)
    {
        _logger.LogDebug($"UsersController::Update id: {id}");

        await _userService.Update(id, model);

        return Ok(
            new { message = "User Updated" }
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogDebug($"UsersController::Delete id: {id}");

        await _userService.Delete(id);

        return Ok(
            new { message = "User Deleted" }
        );
    }
}
