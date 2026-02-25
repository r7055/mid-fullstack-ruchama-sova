using RealTimeMonitor.Models;

namespace RealTimeMonitor.Services;

public interface ITransactionService
{
    Task ProcessAsync(Transaction transaction);
}