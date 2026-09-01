using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Worker;

public sealed class PaymentProcessedConsumer(
    IOptions<RabbitMqOptions> options,
    ILogger<PaymentProcessedConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = "notification-worker-consumer"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            _options.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(
            _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(
            _options.Queue,
            _options.Exchange,
            _options.RoutingKey,
            cancellationToken: stoppingToken);
        await _channel.BasicQosAsync(0, prefetchCount: 10, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(delivery.Body.Span);
                logger.LogInformation("Payment processed notification received: {Message}", message);
                await _channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Payment notification processing failed");
                await _channel.BasicNackAsync(
                    delivery.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            _options.Queue,
            autoAck: false,
            consumer,
            stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
