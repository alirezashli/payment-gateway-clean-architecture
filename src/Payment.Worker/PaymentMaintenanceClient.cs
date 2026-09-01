using System.Net.Http.Json;

namespace Payment.Worker;

public sealed class PaymentMaintenanceClient(HttpClient httpClient)
{
    public async Task<int> ExpirePendingAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync(
            "api/payment/internal/expire-pending",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExpirationResult>(
            cancellationToken: cancellationToken);
        return result?.ExpiredCount ?? 0;
    }

    private sealed record ExpirationResult(int ExpiredCount);
}
