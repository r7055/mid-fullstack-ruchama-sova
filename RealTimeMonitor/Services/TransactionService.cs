using Microsoft.AspNetCore.SignalR;
using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;
using System.ComponentModel.DataAnnotations;

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
        if (transaction is null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        transaction.Currency = transaction.Currency.Trim().ToUpperInvariant();
        Validate(transaction);

        transaction.Timestamp = DateTime.UtcNow;

        _repository.Add(transaction);

        await _hubContext.Clients.All
            .SendAsync(TransactionHub.ReceiveTransactionMethod, transaction);
    }

    private static void Validate(Transaction transaction)
    {
        var validationContext = new ValidationContext(transaction);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            transaction,
            validationContext,
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            var errorMessage = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
            throw new ValidationException(errorMessage);
        }
    }
}
