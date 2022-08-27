using Microsoft.AspNetCore.Mvc;

namespace web_ui.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : Controller
{
    
    [HttpGet("")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }
}
