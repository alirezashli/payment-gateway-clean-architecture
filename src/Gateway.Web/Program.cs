using Gateway.Core.Services;
using Gateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["PaymentService:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddScoped<GatewayService>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.Run();
