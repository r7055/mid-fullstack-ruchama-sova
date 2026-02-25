namespace RealTimeMonitor.Models;

public class Transaction
{
    public Guid TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public TransactionStatus Status { get; set; }

    public DateTime Timestamp { get; set; }
}