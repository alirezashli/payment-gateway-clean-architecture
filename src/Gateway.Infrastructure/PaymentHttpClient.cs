using System.Net.Http.Json;
using Gateway.Core.Services;

namespace Gateway.Infrastructure;

public sealed class PaymentHttpClient(HttpClient client) : IPaymentClient
{
    public async Task<PaymentInfo?> GetAsync(Guid token, CancellationToken ct)
    {
        var response = await client.GetAsync($"api/payment/internal/{token}", ct);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PaymentInfo>(cancellationToken: ct) : null;
    }

    public async Task<bool> UpdateAsync(Guid token, bool isSuccess, string? rrn, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync("api/payment/update-status", new { token, isSuccess, rrn }, ct);
        return response.IsSuccessStatusCode;
    }
}
