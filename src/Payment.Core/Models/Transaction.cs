namespace Payment.Core.Models;

public enum PaymentStatus { Pending, Success, Failed, Expired }

public sealed class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string TerminalNo { get; init; }
    public long Amount { get; init; }
    public required string RedirectUrl { get; init; }
    public required string ReservationNumber { get; init; }
    public required string PhoneNumber { get; init; }
    public Guid Token { get; init; } = Guid.NewGuid();
    public string? Rrn { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public string? AppCode { get; private set; }

    public bool IsExpired(DateTime now) => Status == PaymentStatus.Pending && now - CreatedAt >= TimeSpan.FromMinutes(2);
    public void Expire() { if (Status == PaymentStatus.Pending) SetStatus(PaymentStatus.Expired); }
    public void Complete(bool successful, string? rrn)
    {
        if (Status != PaymentStatus.Pending) throw new InvalidOperationException("تراکنش دیگر قابل تغییر نیست");
        Rrn = successful ? rrn : null;
        SetStatus(successful ? PaymentStatus.Success : PaymentStatus.Failed);
    }
    public void SetAppCode(string appCode) { AppCode = appCode; UpdatedAt = DateTime.UtcNow; }
    private void SetStatus(PaymentStatus status) { Status = status; UpdatedAt = DateTime.UtcNow; }
}
