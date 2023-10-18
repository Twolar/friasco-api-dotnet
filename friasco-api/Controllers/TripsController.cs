using Microsoft.AspNetCore.Mvc;

namespace friasco_api.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsController : ControllerBase
{
    private readonly ILogger<TripsController> _logger;

    public TripsController(ILogger<TripsController> logger)
    {
        _logger = logger;
    }

    // Create
    // Read
    // Update
    // Delete
}
