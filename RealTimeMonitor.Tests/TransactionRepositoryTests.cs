using FluentAssertions;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;

namespace RealTimeMonitor.Tests;

public class TransactionRepositoryTests
{
    [Fact]
    public void Add_ShouldStoreTransaction()
    {
        var repo = new TransactionRepository();

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 100,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };

        repo.Add(transaction);

        repo.GetAll().Should().ContainSingle();
    }

    [Fact]
    public void Repository_ShouldNotExceed_1000_Items()
    {
        var repo = new TransactionRepository();

        for (int i = 0; i < 1100; i++)
        {
            repo.Add(new Transaction
            {
                TransactionId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Status = TransactionStatus.Completed
            });
        }

        repo.GetAll().Count().Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public async Task Add_ShouldBeThreadSafe_UnderConcurrency()
    {
        var repo = new TransactionRepository();
        const int tasksCount = 20;
        const int itemsPerTask = 100;

        var tasks = Enumerable.Range(0, tasksCount).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < itemsPerTask; i++)
            {
                repo.Add(new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    Amount = 100,
                    Currency = "USD",
                    Status = TransactionStatus.Completed
                });
            }
        }));

        await Task.WhenAll(tasks);

        repo.GetAll().Count().Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public void Repository_ShouldKeepLatest_1000_Transactions()
    {
        var repo = new TransactionRepository();
        var insertedIds = new List<Guid>();

        for (int i = 0; i < 1100; i++)
        {
            var id = Guid.NewGuid();
            insertedIds.Add(id);

            repo.Add(new Transaction
            {
                TransactionId = id,
                Amount = 100,
                Currency = "USD",
                Status = TransactionStatus.Completed
            });
        }

        var storedIds = repo.GetAll().Select(t => t.TransactionId).ToList();
        var expectedIds = insertedIds.Skip(100).ToList();

        storedIds.Should().Equal(expectedIds);
    }
}
