namespace AtlasBank.Core.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = "ZAR")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "ZAR") => new(0, currency);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");
        
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");
        
        return a.Amount < b.Amount;
    }

    public Money MultiplyBy(decimal multiplier) => new(Amount * multiplier, Currency);
}
