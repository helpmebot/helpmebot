namespace Helpmebot.WebUI.Controllers;

using Microsoft.AspNetCore.Mvc;

public class HealthController : Controller
{
    public HealthController()
    {
    }
    
    [HttpGet("/health/readiness")]
    public IActionResult Readiness()
    {
        return this.Ok("OK");
    }
    
    [HttpGet("/health/startup")]
    public IActionResult Startup()
    {
        return this.Ok("OK");
    }
}