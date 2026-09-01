using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<PaymentMaintenanceClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddHostedService<PaymentExpirationWorker>();

var host = builder.Build();
await host.RunAsync();
