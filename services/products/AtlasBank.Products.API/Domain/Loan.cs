using AtlasBank.Core.Domain.Common;
using AtlasBank.Core.Domain.ValueObjects;

namespace AtlasBank.Products.API.Domain;

public class Loan : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public string LoanNumber { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Money PrincipalAmount { get; private set; } = Money.Zero();
    public Money OutstandingBalance { get; private set; } = Money.Zero();
    public decimal InterestRate { get; private set; }
    public int TermMonths { get; private set; }
    public int RemainingMonths { get; private set; }
    public Money MonthlyPayment { get; private set; } = Money.Zero();
    public DateTime DisbursementDate { get; private set; }
    public DateTime? MaturityDate { get; private set; }
    public DateTime NextPaymentDate { get; private set; }
    public LoanStatus Status { get; private set; }

    private Loan() { }

    public static Loan Create(
        string tenantId,
        string customerId,
        string accountId,
        Guid productId,
        Money principalAmount,
        decimal interestRate,
        int termMonths,
        string createdBy)
    {
        var monthlyPayment = CalculateMonthlyPayment(principalAmount, interestRate, termMonths);

        var loan = new Loan
        {
            TenantId = tenantId,
            CustomerId = customerId,
            AccountId = accountId,
            LoanNumber = GenerateLoanNumber(),
            ProductId = productId,
            PrincipalAmount = principalAmount,
            OutstandingBalance = principalAmount,
            InterestRate = interestRate,
            TermMonths = termMonths,
            RemainingMonths = termMonths,
            MonthlyPayment = monthlyPayment,
            Status = LoanStatus.Pending,
            CreatedBy = createdBy
        };

        loan.AddDomainEvent(new LoanCreatedEvent(loan.Id, loan.LoanNumber, customerId, principalAmount));
        return loan;
    }

    public void Approve(DateTime disbursementDate, string approvedBy)
    {
        if (Status != LoanStatus.Pending)
            throw new InvalidOperationException("Only pending loans can be approved");

        Status = LoanStatus.Approved;
        DisbursementDate = disbursementDate;
        MaturityDate = disbursementDate.AddMonths(TermMonths);
        NextPaymentDate = disbursementDate.AddMonths(1);
        UpdateTimestamp(approvedBy);
        AddDomainEvent(new LoanApprovedEvent(Id, LoanNumber, DisbursementDate));
    }

    public void Disburse(string disbursedBy)
    {
        if (Status != LoanStatus.Approved)
            throw new InvalidOperationException("Only approved loans can be disbursed");

        Status = LoanStatus.Active;
        UpdateTimestamp(disbursedBy);
        AddDomainEvent(new LoanDisbursedEvent(Id, LoanNumber, PrincipalAmount));
    }

    public void MakePayment(Money paymentAmount, string paidBy)
    {
        if (Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can receive payments");

        if (paymentAmount > OutstandingBalance)
            paymentAmount = OutstandingBalance;

        OutstandingBalance -= paymentAmount;
        RemainingMonths--;
        NextPaymentDate = NextPaymentDate.AddMonths(1);
        UpdateTimestamp(paidBy);

        if (OutstandingBalance.Amount <= 0)
        {
            Status = LoanStatus.PaidOff;
            AddDomainEvent(new LoanPaidOffEvent(Id, LoanNumber));
        }
        else
        {
            AddDomainEvent(new LoanPaymentMadeEvent(Id, LoanNumber, paymentAmount, OutstandingBalance));
        }
    }

    public void MarkAsDefaulted(string reason, string markedBy)
    {
        Status = LoanStatus.Defaulted;
        UpdateTimestamp(markedBy);
        AddDomainEvent(new LoanDefaultedEvent(Id, LoanNumber, reason));
    }

    private static Money CalculateMonthlyPayment(Money principal, decimal annualRate, int months)
    {
        if (annualRate == 0) return new Money(principal.Amount / months, principal.Currency);

        var monthlyRate = (decimal)(annualRate / 12 / 100);
        var payment = principal.Amount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months)) /
                      (decimal)(Math.Pow((double)(1 + monthlyRate), months) - 1);

        return new Money(payment, principal.Currency);
    }

    private static string GenerateLoanNumber()
    {
        return $"LN{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}

public enum LoanStatus
{
    Pending = 0,
    Approved = 1,
    Active = 2,
    PaidOff = 3,
    Defaulted = 4,
    WrittenOff = 5
}

// Domain Events
public record LoanCreatedEvent(Guid LoanId, string LoanNumber, string CustomerId, Money Amount) : DomainEvent;
public record LoanApprovedEvent(Guid LoanId, string LoanNumber, DateTime DisbursementDate) : DomainEvent;
public record LoanDisbursedEvent(Guid LoanId, string LoanNumber, Money Amount) : DomainEvent;
public record LoanPaymentMadeEvent(Guid LoanId, string LoanNumber, Money PaymentAmount, Money RemainingBalance) : DomainEvent;
public record LoanPaidOffEvent(Guid LoanId, string LoanNumber) : DomainEvent;
public record LoanDefaultedEvent(Guid LoanId, string LoanNumber, string Reason) : DomainEvent;
