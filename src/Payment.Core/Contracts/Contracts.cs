using System.ComponentModel.DataAnnotations;
using Payment.Core.Models;

namespace Payment.Core.Contracts;

public sealed record GetTokenRequest(
    [Required] string TerminalNo,
    [Range(typeof(long), "1", "9223372036854775807")] long Amount,
    [Required, Url] string RedirectUrl,
    [Required] string ReservationNumber,
    [Required, RegularExpression(@"^09\d{9}$")] string PhoneNumber);
public sealed record VerifyRequest(Guid Token, [Required] string AppCode);
public sealed record UpdateStatusRequest(Guid Token, bool IsSuccess, string? Rrn);
public sealed record TransactionView(Guid Token, long Amount, string RedirectUrl, string ReservationNumber, string Status);
public sealed record PaymentProcessedEvent(Guid Token, long Amount, PaymentStatus Status, string? Rrn, string RedirectUrl);

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct);
    Task<Transaction?> FindAsync(Guid token, CancellationToken ct);
    Task<IReadOnlyList<Transaction>> GetPendingAsync(CancellationToken ct);
}
public interface IEventPublisher { Task PublishAsync(PaymentProcessedEvent message, CancellationToken ct); }
