using Payment.Core.Contracts;
using Payment.Core.Models;

namespace Payment.Core.Services;

public sealed class PaymentService(ITransactionRepository repository, IEventPublisher events)
{
    public async Task<object> GetTokenAsync(GetTokenRequest request, CancellationToken ct)
    {
        var transaction = new Transaction
        {
            TerminalNo = request.TerminalNo,
            Amount = request.Amount,
            RedirectUrl = request.RedirectUrl,
            ReservationNumber = request.ReservationNumber,
            PhoneNumber = request.PhoneNumber
        };
        await repository.AddAsync(transaction, ct);
        return new { isSuccess = true, gatewayUrl = $"http://localhost:5002/api/gateway/pay/{transaction.Token}", token = transaction.Token, message = "توکن با موفقیت ایجاد شد" };
    }

    public async Task<object?> VerifyAsync(VerifyRequest request, CancellationToken ct)
    {
        var transaction = await repository.FindAsync(request.Token, ct);
        if (transaction is null) return null;
        if (transaction.IsExpired(DateTime.UtcNow)) transaction.Expire();
        transaction.SetAppCode(request.AppCode);
        return new { isSuccess = transaction.Status == PaymentStatus.Success, status = transaction.Status.ToString(), transaction.Amount, rrn = transaction.Rrn, transaction.ReservationNumber, message = Message(transaction.Status) };
    }

    public async Task<bool> UpdateStatusAsync(UpdateStatusRequest request, CancellationToken ct)
    {
        var transaction = await repository.FindAsync(request.Token, ct);
        if (transaction is null) return false;
        if (transaction.IsExpired(DateTime.UtcNow)) transaction.Expire();
        if (transaction.Status != PaymentStatus.Pending) return false;
        transaction.Complete(request.IsSuccess, request.Rrn);
        await events.PublishAsync(new(transaction.Token, transaction.Amount, transaction.Status, transaction.Rrn, transaction.RedirectUrl), ct);
        return true;
    }

    public async Task<TransactionView?> GetAsync(Guid token, CancellationToken ct)
    {
        var transaction = await repository.FindAsync(token, ct);
        if (transaction is null) return null;
        if (transaction.IsExpired(DateTime.UtcNow)) transaction.Expire();
        return new(transaction.Token, transaction.Amount, transaction.RedirectUrl, transaction.ReservationNumber, transaction.Status.ToString());
    }

    public async Task<int> ExpirePendingAsync(CancellationToken ct)
    {
        var pending = await repository.GetPendingAsync(ct);
        var expired = pending.Where(transaction => transaction.IsExpired(DateTime.UtcNow)).ToList();
        expired.ForEach(transaction => transaction.Expire());
        return expired.Count;
    }

    private static string Message(PaymentStatus status) => status switch
    {
        PaymentStatus.Success => "پرداخت با موفقیت تایید شد",
        PaymentStatus.Failed => "پرداخت ناموفق بود",
        PaymentStatus.Expired => "زمان پرداخت منقضی شده است",
        _ => "پرداخت هنوز انجام نشده است"
    };
}
