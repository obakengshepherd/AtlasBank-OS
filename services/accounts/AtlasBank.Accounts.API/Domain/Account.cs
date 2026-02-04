using AtlasBank.Core.Domain.Common;
using AtlasBank.Core.Domain.ValueObjects;
using AtlasBank.Core.Domain.Enums;

namespace AtlasBank.Accounts.API.Domain;

public class Account : AggregateRoot
{
    public string AccountNumber { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public string TenantId { get; private set; } = string.Empty;
    public ProductType ProductType { get; private set; }
    public Money Balance { get; private set; } = Money.Zero();
    public Money AvailableBalance { get; private set; } = Money.Zero();
    public AccountStatus Status { get; private set; }
    public decimal InterestRate { get; private set; }
    public DateTime? LastInterestDate { get; private set; }

    private Account() { } // EF Core

    public static Account Create(
        string customerId, 
        string tenantId, 
        ProductType productType,
        decimal interestRate,
        string createdBy)
    {
        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),
            CustomerId = customerId,
            TenantId = tenantId,
            ProductType = productType,
            Balance = Money.Zero(),
            AvailableBalance = Money.Zero(),
            Status = AccountStatus.Pending,
            InterestRate = interestRate,
            CreatedBy = createdBy
        };

        account.AddDomainEvent(new AccountCreatedEvent(account.Id, account.AccountNumber, customerId, tenantId));
        return account;
    }

    public void Activate(string activatedBy)
    {
        if (Status != AccountStatus.Pending)
            throw new InvalidOperationException("Only pending accounts can be activated");

        Status = AccountStatus.Active;
        UpdateTimestamp(activatedBy);
        AddDomainEvent(new AccountActivatedEvent(Id, AccountNumber));
    }

    public void Deposit(Money amount, string performedBy)
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account must be active for deposits");

        Balance += amount;
        AvailableBalance += amount;
        UpdateTimestamp(performedBy);
        AddDomainEvent(new AccountDepositedEvent(Id, AccountNumber, amount));
    }

    public void Withdraw(Money amount, string performedBy)
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account must be active for withdrawals");

        if (AvailableBalance < amount)
            throw new InvalidOperationException("Insufficient funds");

        Balance -= amount;
        AvailableBalance -= amount;
        UpdateTimestamp(performedBy);
        AddDomainEvent(new AccountWithdrawnEvent(Id, AccountNumber, amount));
    }

    public void Freeze(string reason, string performedBy)
    {
        Status = AccountStatus.Frozen;
        UpdateTimestamp(performedBy);
        AddDomainEvent(new AccountFrozenEvent(Id, AccountNumber, reason));
    }

    public void ApplyInterest(string performedBy)
    {
        if (InterestRate <= 0) return;

        var interestAmount = Balance.MultiplyBy(InterestRate / 100 / 12);
        Balance += interestAmount;
        AvailableBalance += interestAmount;
        LastInterestDate = DateTime.UtcNow;
        UpdateTimestamp(performedBy);
        AddDomainEvent(new InterestAppliedEvent(Id, AccountNumber, interestAmount));
    }

    private static string GenerateAccountNumber()
    {
        var random = new Random();
        return $"62{random.Next(10000000, 99999999)}";
    }
}

// Domain Events
public record AccountCreatedEvent(Guid AccountId, string AccountNumber, string CustomerId, string TenantId) : DomainEvent;
public record AccountActivatedEvent(Guid AccountId, string AccountNumber) : DomainEvent;
public record AccountDepositedEvent(Guid AccountId, string AccountNumber, Money Amount) : DomainEvent;
public record AccountWithdrawnEvent(Guid AccountId, string AccountNumber, Money Amount) : DomainEvent;
public record AccountFrozenEvent(Guid AccountId, string AccountNumber, string Reason) : DomainEvent;
public record InterestAppliedEvent(Guid AccountId, string AccountNumber, Money Amount) : DomainEvent;
