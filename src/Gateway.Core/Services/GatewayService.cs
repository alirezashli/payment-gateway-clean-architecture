using Gateway.Core.Models;

namespace Gateway.Core.Services;

public sealed record PaymentInfo(Guid Token, long Amount, string RedirectUrl, string ReservationNumber, string Status);
public interface IPaymentClient
{
    Task<PaymentInfo?> GetAsync(Guid token, CancellationToken ct);
    Task<bool> UpdateAsync(Guid token, bool isSuccess, string? rrn, CancellationToken ct);
}

public sealed class GatewayService(IPaymentClient paymentClient)
{
    public async Task<PaymentResult?> PayAsync(Guid token, CancellationToken ct)
    {
        var payment = await paymentClient.GetAsync(token, ct);
        if (payment is null || payment.Status != "Pending") return null;
        var success = Random.Shared.Next(100) < 80;
        var rrn = success ? Random.Shared.NextInt64(100_000_000_000, 999_999_999_999).ToString() : null;
        if (!await paymentClient.UpdateAsync(token, success, rrn, ct)) return null;
        return new(success, token, rrn, payment.Amount, success ? "پرداخت با موفقیت انجام شد" : "پرداخت ناموفق بود", payment.RedirectUrl);
    }
}
