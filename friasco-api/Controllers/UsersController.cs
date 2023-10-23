using Microsoft.AspNetCore.Mvc;

namespace friasco_api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    // Create
    // Read all / byId
    // Update
    // Delete
}
