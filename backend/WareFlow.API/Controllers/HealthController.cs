using Microsoft.AspNetCore.Mvc;

namespace WareFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            status = "OK", 
            message = "WareFlow API is running!", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
