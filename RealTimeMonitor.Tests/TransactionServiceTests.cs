using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;
using RealTimeMonitor.Services;
using System.ComponentModel.DataAnnotations;

namespace RealTimeMonitor.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task ProcessAsync_Should_Add_Transaction_And_Broadcast()
    {
        var repo = new Mock<ITransactionRepository>();

        var hubContext = new Mock<IHubContext<TransactionHub>>();
        var clients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(x => x.All).Returns(clientProxy.Object);
        hubContext.Setup(x => x.Clients).Returns(clients.Object);

        var service = new TransactionService(repo.Object, hubContext.Object);
        var transaction = CreateValidTransaction();

        await service.ProcessAsync(transaction);

        repo.Verify(r => r.Add(It.Is<Transaction>(t => ReferenceEquals(t, transaction))), Times.Once);
        clientProxy.Verify(
            c => c.SendCoreAsync(
                TransactionHub.ReceiveTransactionMethod,
                It.Is<object?[]>(args => args.Length == 1 && ReferenceEquals(args[0], transaction)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_Should_SetTimestamp_ToUtcNow()
    {
        var repo = new Mock<ITransactionRepository>();

        var hubContext = new Mock<IHubContext<TransactionHub>>();
        var clients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(x => x.All).Returns(clientProxy.Object);
        hubContext.Setup(x => x.Clients).Returns(clients.Object);

        var service = new TransactionService(repo.Object, hubContext.Object);
        var transaction = CreateValidTransaction();

        var before = DateTime.UtcNow;
        await service.ProcessAsync(transaction);
        var after = DateTime.UtcNow;

        transaction.Timestamp.Should().BeOnOrAfter(before);
        transaction.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task ProcessAsync_Should_NormalizeCurrency_ToUppercase()
    {
        var repo = new Mock<ITransactionRepository>();

        var hubContext = new Mock<IHubContext<TransactionHub>>();
        var clients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(x => x.All).Returns(clientProxy.Object);
        hubContext.Setup(x => x.Clients).Returns(clients.Object);

        var service = new TransactionService(repo.Object, hubContext.Object);
        var transaction = CreateValidTransaction();
        transaction.Currency = " usd ";

        await service.ProcessAsync(transaction);

        transaction.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task ProcessAsync_Should_ThrowValidationException_WhenTransactionInvalid()
    {
        var repo = new Mock<ITransactionRepository>();

        var hubContext = new Mock<IHubContext<TransactionHub>>();
        var clients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(x => x.All).Returns(clientProxy.Object);
        hubContext.Setup(x => x.Clients).Returns(clients.Object);

        var service = new TransactionService(repo.Object, hubContext.Object);
        var transaction = CreateValidTransaction();
        transaction.Amount = 0;

        Func<Task> act = () => service.ProcessAsync(transaction);

        await act.Should().ThrowAsync<ValidationException>();

        repo.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Never);
        clientProxy.Verify(
            c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_Should_ThrowArgumentNullException_WhenTransactionIsNull()
    {
        var repo = new Mock<ITransactionRepository>();

        var hubContext = new Mock<IHubContext<TransactionHub>>();
        var clients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(x => x.All).Returns(clientProxy.Object);
        hubContext.Setup(x => x.Clients).Returns(clients.Object);

        var service = new TransactionService(repo.Object, hubContext.Object);

        Func<Task> act = () => service.ProcessAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();

        repo.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_Should_Handle_100_ConcurrentAdds_WithoutErrors()
    {
        var repo = new TransactionRepository();

        var hubContext = new Mock<IHubContext<TransactionHub>>();
        var clients = new Mock<IHubClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(x => x.All).Returns(clientProxy.Object);
        hubContext.Setup(x => x.Clients).Returns(clients.Object);

        var service = new TransactionService(repo, hubContext.Object);

        var tasks = Enumerable.Range(0, 100).Select(_ => service.ProcessAsync(CreateValidTransaction()));

        await Task.WhenAll(tasks);

        var stored = repo.GetAll().ToList();

        stored.Should().HaveCount(100);
        stored.Should().OnlyContain(t => t.Timestamp != default);
    }

    private static Transaction CreateValidTransaction()
    {
        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 100,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };
    }
}
