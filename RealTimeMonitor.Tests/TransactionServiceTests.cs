using Microsoft.AspNetCore.SignalR;
using Moq;
using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;
using RealTimeMonitor.Repositories;
using RealTimeMonitor.Services;

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

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 100,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };

        await service.ProcessAsync(transaction);

        repo.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Once);
        clientProxy.Verify(
            c => c.SendCoreAsync(
                "ReceiveTransaction",
                It.Is<object?[]>(args => args.Length == 1 && ReferenceEquals(args[0], transaction)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
