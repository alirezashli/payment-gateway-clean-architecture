namespace Notification.Worker;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string ConnectionString { get; init; } = "amqp://guest:guest@localhost:5672";
    public string Exchange { get; init; } = "payments";
    public string Queue { get; init; } = "notifications.payment-processed";
    public string RoutingKey { get; init; } = "payment.processed";
}
