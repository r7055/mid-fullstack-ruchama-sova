using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using RealTimeMonitor.Models;

namespace RealTimeMonitor.Tests;

public class TransactionValidationTests
{
    [Fact]
    public void Transaction_WithNegativeAmount_ShouldBeInvalid()
    {
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = -100,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };

        var context = new ValidationContext(transaction);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(transaction, context, results, true);

        isValid.Should().BeFalse();
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void Transaction_WithValidData_ShouldBeValid()
    {
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            Amount = 150,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };

        var context = new ValidationContext(transaction);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(transaction, context, results, true);

        isValid.Should().BeTrue();
    }
}