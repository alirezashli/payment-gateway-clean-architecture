using System.Collections.Concurrent;
using Payment.Core.Contracts;
using Payment.Core.Models;

namespace Payment.Infrastructure;

public sealed class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<Guid, Transaction> _items = new();
    public Task AddAsync(Transaction transaction, CancellationToken ct) { _items[transaction.Token] = transaction; return Task.CompletedTask; }
    public Task<Transaction?> FindAsync(Guid token, CancellationToken ct) { _items.TryGetValue(token, out var item); return Task.FromResult(item); }
    public Task<IReadOnlyList<Transaction>> GetPendingAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Transaction>>(_items.Values.Where(x => x.Status == PaymentStatus.Pending).ToList());
}
