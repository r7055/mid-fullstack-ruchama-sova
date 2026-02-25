using System.ComponentModel.DataAnnotations;

namespace RealTimeMonitor.Models;

public class Transaction
{
    [Required]
    public Guid TransactionId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 letters")]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public TransactionStatus Status { get; set; }

    public DateTime Timestamp { get; set; }
}