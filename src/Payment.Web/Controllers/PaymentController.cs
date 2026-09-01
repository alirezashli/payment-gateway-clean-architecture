using Microsoft.AspNetCore.Mvc;
using Payment.Core.Contracts;
using Payment.Core.Services;

namespace Payment.Web.Controllers;

[ApiController]
[Route("api/payment")]
public sealed class PaymentController(PaymentService paymentService) : ControllerBase
{
    [HttpPost("get-token")]
    public async Task<IActionResult> GetToken([FromBody] GetTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await paymentService.GetTokenAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request, CancellationToken cancellationToken)
    {
        var result = await paymentService.VerifyAsync(request, cancellationToken);
        return result is null
            ? BadRequest(new { isSuccess = false, message = "توکن نامعتبر است" })
            : Ok(result);
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await paymentService.UpdateStatusAsync(request, cancellationToken);
        return updated
            ? Ok(new { isSuccess = true, message = "وضعیت با موفقیت به‌روزرسانی شد" })
            : BadRequest(new { isSuccess = false, message = "توکن یا وضعیت نامعتبر است" });
    }

    [HttpGet("internal/{token:guid}")]
    public async Task<IActionResult> GetInternal([FromRoute] Guid token, CancellationToken cancellationToken)
    {
        var result = await paymentService.GetAsync(token, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("internal/expire-pending")]
    public async Task<IActionResult> ExpirePending(CancellationToken cancellationToken)
    {
        var expiredCount = await paymentService.ExpirePendingAsync(cancellationToken);
        return Ok(new { expiredCount });
    }
}
