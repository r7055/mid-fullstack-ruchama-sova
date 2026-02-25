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

        repo.GetAll().Count().Should().BeLessOrEqualTo(1000);
    }
}