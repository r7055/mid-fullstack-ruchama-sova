using Microsoft.AspNetCore.SignalR;
using Moq;
using RealTimeMonitor.Hubs;
using RealTimeMonitor.Models;

namespace RealTimeMonitor.Tests;

public class TransactionHubTests
{
    [Fact]
    public async Task BroadcastTransaction_ShouldSendToAllClients()
    {
        var clients = new Mock<IHubCallerClients>();
        var clientProxy = new Mock<IClientProxy>();

        clients.Setup(c => c.All).Returns(clientProxy.Object);

        var hub = new TransactionHub
        {
            Clients = clients.Object
        };

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 100,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };

        await hub.BroadcastTransaction(transaction);

        clientProxy.Verify(
            c => c.SendCoreAsync(
                TransactionHub.ReceiveTransactionMethod,
                It.Is<object?[]>(args => args.Length == 1 && ReferenceEquals(args[0], transaction)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
