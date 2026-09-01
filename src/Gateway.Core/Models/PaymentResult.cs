namespace Gateway.Core.Models;

public sealed record PaymentResult(bool IsSuccess, Guid Token, string? Rrn, long Amount, string Message, string RedirectUrl);
