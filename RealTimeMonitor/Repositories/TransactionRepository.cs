using System.Collections.Concurrent;
using RealTimeMonitor.Models;

namespace RealTimeMonitor.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ConcurrentQueue<Transaction> _transactions = new();

    public void Add(Transaction transaction)
    {
        _transactions.Enqueue(transaction);

        // נשמור מקסימום 1000
        while (_transactions.Count > 1000)
        {
            _transactions.TryDequeue(out _);
        }
    }

    public IEnumerable<Transaction> GetAll()
    {
        return _transactions.ToList();
    }
}