using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHostedService<PaymentProcessedConsumer>();

await builder.Build().RunAsync();
