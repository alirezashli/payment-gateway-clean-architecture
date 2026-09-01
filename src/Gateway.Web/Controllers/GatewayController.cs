using Gateway.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Web.Controllers;

[ApiController]
[Route("api/gateway")]
public sealed class GatewayController(GatewayService gatewayService) : ControllerBase
{
    [HttpGet("pay/{token:guid}")]
    public async Task<IActionResult> Pay([FromRoute] Guid token, CancellationToken cancellationToken)
    {
        var result = await gatewayService.PayAsync(token, cancellationToken);
        return result is null
            ? BadRequest(new { isSuccess = false, message = "توکن یا وضعیت پرداخت نامعتبر است" })
            : Ok(result);
    }
}
