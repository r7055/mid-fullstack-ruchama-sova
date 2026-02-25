using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;
using RealTimeMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

var app = builder.Build();

app.MapPost("/transactions",
    async (Transaction transaction,
           ITransactionService service) =>
{
    await service.ProcessAsync(transaction);
    return Results.Ok();
});

app.MapHub<TransactionHub>("/hub/transactions");

app.Run();