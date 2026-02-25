using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;
using RealTimeMonitor.Services;
using System.ComponentModel.DataAnnotations;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

var app = builder.Build();

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

app.MapHub<TransactionHub>("/hub/transactions");

app.Run();