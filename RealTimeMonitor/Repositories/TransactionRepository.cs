using RealTimeMonitor.Models;

namespace RealTimeMonitor.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private const int MaxTransactions = 1000;
    private readonly Queue<Transaction> _transactions = new();
    private readonly object _sync = new();

    public void Add(Transaction transaction)
    {
        lock (_sync)
        {
            _transactions.Enqueue(transaction);

            while (_transactions.Count > MaxTransactions)
            {
                _transactions.Dequeue();
            }
        }
    }

    public IEnumerable<Transaction> GetAll()
    {
        lock (_sync)
        {
            return _transactions.ToList();
        }
    }
}
