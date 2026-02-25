using RealTimeMonitor.Models;

namespace RealTimeMonitor.Repositories;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
    IEnumerable<Transaction> GetAll();
}