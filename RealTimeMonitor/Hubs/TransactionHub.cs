using Microsoft.AspNetCore.SignalR;
using RealTimeMonitor.Models;

namespace RealTimeMonitor.Hubs;

public class TransactionHub : Hub
{
    public const string ReceiveTransactionMethod = "ReceiveTransaction";

    public Task BroadcastTransaction(Transaction transaction)
    {
        return Clients.All.SendAsync(ReceiveTransactionMethod, transaction);
    }
}
