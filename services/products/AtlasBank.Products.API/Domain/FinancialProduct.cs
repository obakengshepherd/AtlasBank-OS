using AtlasBank.Core.Domain.Common;
using AtlasBank.Core.Domain.ValueObjects;
using AtlasBank.Core.Domain.Enums;

namespace AtlasBank.Products.API.Domain;

public class FinancialProduct : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public ProductType ProductType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal InterestRate { get; private set; }
    public decimal MinimumBalance { get; private set; }
    public decimal MaximumBalance { get; private set; }
    public Money MonthlyFee { get; private set; } = Money.Zero();
    public int TermMonths { get; private set; }
    public bool IsActive { get; private set; }
    public Dictionary<string, string> Features { get; private set; } = new();

    private FinancialProduct() { }

    public static FinancialProduct Create(
        string tenantId,
        string productCode,
        string productName,
        ProductType productType,
        string description,
        decimal interestRate,
        decimal minimumBalance,
        decimal maximumBalance,
        Money monthlyFee,
        int termMonths,
        Dictionary<string, string> features,
        string createdBy)
    {
        var product = new FinancialProduct
        {
            TenantId = tenantId,
            ProductCode = productCode,
            ProductName = productName,
            ProductType = productType,
            Description = description,
            InterestRate = interestRate,
            MinimumBalance = minimumBalance,
            MaximumBalance = maximumBalance,
            MonthlyFee = monthlyFee,
            TermMonths = termMonths,
            Features = features,
            IsActive = true,
            CreatedBy = createdBy
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, productCode, productName, productType));
        return product;
    }

    public void UpdateInterestRate(decimal newRate, string updatedBy)
    {
        if (newRate < 0)
            throw new ArgumentException("Interest rate cannot be negative");

        var oldRate = InterestRate;
        InterestRate = newRate;
        UpdateTimestamp(updatedBy);
        AddDomainEvent(new ProductInterestRateChangedEvent(Id, ProductCode, oldRate, newRate));
    }

    public void UpdateFees(Money newFee, string updatedBy)
    {
        var oldFee = MonthlyFee;
        MonthlyFee = newFee;
        UpdateTimestamp(updatedBy);
        AddDomainEvent(new ProductFeeChangedEvent(Id, ProductCode, oldFee, newFee));
    }

    public void Deactivate(string deactivatedBy)
    {
        IsActive = false;
        UpdateTimestamp(deactivatedBy);
        AddDomainEvent(new ProductDeactivatedEvent(Id, ProductCode));
    }

    public void Activate(string activatedBy)
    {
        IsActive = true;
        UpdateTimestamp(activatedBy);
        AddDomainEvent(new ProductActivatedEvent(Id, ProductCode));
    }

    public void AddFeature(string key, string value, string updatedBy)
    {
        Features[key] = value;
        UpdateTimestamp(updatedBy);
    }

    public bool IsEligible(decimal balance)
    {
        return IsActive && balance >= MinimumBalance && balance <= MaximumBalance;
    }
}

// Domain Events
public record ProductCreatedEvent(Guid ProductId, string ProductCode, string ProductName, ProductType Type) : DomainEvent;
public record ProductInterestRateChangedEvent(Guid ProductId, string ProductCode, decimal OldRate, decimal NewRate) : DomainEvent;
public record ProductFeeChangedEvent(Guid ProductId, string ProductCode, Money OldFee, Money NewFee) : DomainEvent;
public record ProductDeactivatedEvent(Guid ProductId, string ProductCode) : DomainEvent;
public record ProductActivatedEvent(Guid ProductId, string ProductCode) : DomainEvent;
