using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;
using RealTimeMonitor.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddSignalR();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host is "localhost" or "127.0.0.1" or "::1";
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();
app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/transactions",
    async (Transaction transaction,
           ITransactionService service) =>
{
    var validationContext = new ValidationContext(transaction);
    var validationResults = new List<ValidationResult>();

    bool isValid = Validator.TryValidateObject(
        transaction,
        validationContext,
        validationResults,
        true);

    if (!isValid)
    {
        return Results.BadRequest(validationResults);
    }

    await service.ProcessAsync(transaction);

    return Results.Ok();
});

app.MapGet("/transactions", (ITransactionRepository repository) =>
{
    return Results.Ok(repository.GetAll());
});

app.MapHub<TransactionHub>("/hub/transactions");

app.Run();
