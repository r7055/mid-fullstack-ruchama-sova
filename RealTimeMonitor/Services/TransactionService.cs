using Microsoft.AspNetCore.SignalR;
using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;

namespace RealTimeMonitor.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly IHubContext<TransactionHub> _hubContext;

    public TransactionService(
        ITransactionRepository repository,
        IHubContext<TransactionHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    public async Task ProcessAsync(Transaction transaction)
    {
        transaction.Timestamp = DateTime.UtcNow;

        _repository.Add(transaction);

        await _hubContext.Clients.All
            .SendAsync("ReceiveTransaction", transaction);
    }
}