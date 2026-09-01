using System.Text.Json;
using Microsoft.Extensions.Options;
using Payment.Core.Contracts;
using RabbitMQ.Client;

namespace Payment.Infrastructure;

public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync(PaymentProcessedEvent message, CancellationToken cancellationToken)
    {
        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = message.Token.ToString(),
                Type = nameof(PaymentProcessedEvent),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true }) return _channel;

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = "payment-web-publisher"
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);
        await _channel.ExchangeDeclareAsync(
            _options.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        _publishLock.Dispose();
    }
}
