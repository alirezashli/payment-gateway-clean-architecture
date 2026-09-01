using Microsoft.AspNetCore.Mvc;

namespace Payment.Web.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
