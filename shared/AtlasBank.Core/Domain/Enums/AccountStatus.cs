namespace AtlasBank.Core.Domain.Enums;

public enum AccountStatus
{
    Pending = 0,
    Active = 1,
    Frozen = 2,
    Suspended = 3,
    Closed = 4
}

public enum TransactionStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Reversed = 4
}

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Fee = 4,
    Interest = 5,
    Reversal = 6
}

public enum ProductType
{
    CurrentAccount = 1,
    SavingsAccount = 2,
    FixedDeposit = 3,
    PersonalLoan = 4,
    CreditCard = 5,
    MortgageLoan = 6
}

public enum ComplianceCheckType
{
    KYC = 1,
    AML = 2,
    Sanctions = 3,
    PEP = 4,
    TransactionLimit = 5,
    RiskScoring = 6
}

public enum ComplianceStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    UnderReview = 3,
    Escalated = 4
}
