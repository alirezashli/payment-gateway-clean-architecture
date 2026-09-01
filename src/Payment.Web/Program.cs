using Payment.Core.Contracts;
using Payment.Core.Services;
using Payment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.Run();
