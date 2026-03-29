using Microsoft.AspNetCore.Mvc;

namespace WareFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    [HttpPost("test-product")]
    public IActionResult TestProduct([FromBody] dynamic data)
    {
        return Ok(new 
        { 
            received = true,
            data = data,
            type = data.GetType().ToString()
        });
    }
}
