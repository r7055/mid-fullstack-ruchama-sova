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
    public async Task Add_ShouldBeThreadSafe_With100ParallelAdds()
    {
        var repo = new TransactionRepository();
        const int parallelAdds = 100;

        var tasks = Enumerable.Range(0, parallelAdds).Select(_ => Task.Run(() =>
        {
            repo.Add(new Transaction
            {
                TransactionId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Status = TransactionStatus.Completed
            });
        }));

        await Task.WhenAll(tasks);

        repo.GetAll().Should().HaveCount(parallelAdds);
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

    [Fact]
    public void GetAll_ShouldReturnTransactions_InInsertionOrder()
    {
        var repo = new TransactionRepository();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var third = Guid.NewGuid();

        repo.Add(new Transaction
        {
            TransactionId = first,
            Amount = 100,
            Currency = "USD",
            Status = TransactionStatus.Completed
        });
        repo.Add(new Transaction
        {
            TransactionId = second,
            Amount = 200,
            Currency = "USD",
            Status = TransactionStatus.Completed
        });
        repo.Add(new Transaction
        {
            TransactionId = third,
            Amount = 300,
            Currency = "USD",
            Status = TransactionStatus.Completed
        });

        repo.GetAll().Select(t => t.TransactionId).Should().Equal(first, second, third);
    }
}
