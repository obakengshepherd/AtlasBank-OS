using AtlasBank.Core.Domain.Common;
using AtlasBank.Core.Domain.ValueObjects;
using AtlasBank.Core.Domain.Enums;

namespace AtlasBank.Transactions.API.Domain;

public class Transaction : AggregateRoot
{
    public string TransactionReference { get; private set; } = string.Empty;
    public string TenantId { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public string SourceAccountId { get; private set; } = string.Empty;
    public string? DestinationAccountId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public TransactionStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string ProcessedBy { get; private set; } = string.Empty;

    private Transaction() { } // EF Core

    public static Transaction CreateTransfer(
        string tenantId,
        string sourceAccountId,
        string destinationAccountId,
        Money amount,
        string description,
        string createdBy)
    {
        var transaction = new Transaction
        {
            TransactionReference = GenerateReference(),
            TenantId = tenantId,
            Type = TransactionType.Transfer,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = amount,
            Status = TransactionStatus.Pending,
            Description = description,
            CreatedBy = createdBy
        };

        transaction.AddDomainEvent(new TransactionCreatedEvent(
            transaction.Id, 
            transaction.TransactionReference, 
            sourceAccountId, 
            destinationAccountId, 
            amount));

        return transaction;
    }

    public void Process(string processedBy)
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be processed");

        Status = TransactionStatus.Processing;
        ProcessedBy = processedBy;
        UpdateTimestamp(processedBy);
        AddDomainEvent(new TransactionProcessingEvent(Id, TransactionReference));
    }

    public void Complete(string completedBy)
    {
        if (Status != TransactionStatus.Processing)
            throw new InvalidOperationException("Only processing transactions can be completed");

        Status = TransactionStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        ProcessedBy = completedBy;
        UpdateTimestamp(completedBy);
        AddDomainEvent(new TransactionCompletedEvent(Id, TransactionReference, Amount));
    }

    public void Fail(string reason, string failedBy)
    {
        Status = TransactionStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
        ProcessedBy = failedBy;
        UpdateTimestamp(failedBy);
        AddDomainEvent(new TransactionFailedEvent(Id, TransactionReference, reason));
    }

    private static string GenerateReference()
    {
        return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

// Domain Events
public record TransactionCreatedEvent(
    Guid TransactionId, 
    string Reference, 
    string SourceAccountId, 
    string? DestinationAccountId, 
    Money Amount) : DomainEvent;

public record TransactionProcessingEvent(Guid TransactionId, string Reference) : DomainEvent;
public record TransactionCompletedEvent(Guid TransactionId, string Reference, Money Amount) : DomainEvent;
public record TransactionFailedEvent(Guid TransactionId, string Reference, string Reason) : DomainEvent;
