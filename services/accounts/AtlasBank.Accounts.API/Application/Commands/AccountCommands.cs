using AtlasBank.Accounts.API.Domain;
using AtlasBank.Accounts.API.Persistence;
using AtlasBank.Core.Application.Common;
using FluentValidation;
using MediatR;

namespace AtlasBank.Accounts.API.Application.Commands;

// =============== Activate Account Command ===============
public record ActivateAccountCommand(Guid AccountId) : IRequest<Result<Guid>>;

public class ActivateAccountCommandValidator : AbstractValidator<ActivateAccountCommand>
{
    public ActivateAccountCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
    }
}

public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, Result<Guid>>
{
    private readonly AccountsDbContext _context;

    public ActivateAccountCommandHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken: cancellationToken);
        
        if (account == null)
            return Result<Guid>.Failure("Account not found");

        try
        {
            account.Activate("system");
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(account.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// =============== Deposit Command ===============
public record DepositCommand(Guid AccountId, decimal Amount) : IRequest<Result<Guid>>;

public class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}

public class DepositCommandHandler : IRequestHandler<DepositCommand, Result<Guid>>
{
    private readonly AccountsDbContext _context;

    public DepositCommandHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken: cancellationToken);
        
        if (account == null)
            return Result<Guid>.Failure("Account not found");

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            var depositAmount = new Core.Domain.ValueObjects.Money(request.Amount);
            account.Deposit(depositAmount, "system");
            
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return Result<Guid>.Success(account.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// =============== Withdraw Command ===============
public record WithdrawCommand(Guid AccountId, decimal Amount) : IRequest<Result<Guid>>;

public class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}

public class WithdrawCommandHandler : IRequestHandler<WithdrawCommand, Result<Guid>>
{
    private readonly AccountsDbContext _context;

    public WithdrawCommandHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(WithdrawCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken: cancellationToken);
        
        if (account == null)
            return Result<Guid>.Failure("Account not found");

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            var withdrawAmount = new Core.Domain.ValueObjects.Money(request.Amount);
            account.Withdraw(withdrawAmount, "system");
            
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return Result<Guid>.Success(account.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// =============== Freeze Account Command ===============
public record FreezeAccountCommand(Guid AccountId, string Reason) : IRequest<Result<Guid>>;

public class FreezeAccountCommandValidator : AbstractValidator<FreezeAccountCommand>
{
    public FreezeAccountCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required");
    }
}

public class FreezeAccountCommandHandler : IRequestHandler<FreezeAccountCommand, Result<Guid>>
{
    private readonly AccountsDbContext _context;

    public FreezeAccountCommandHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(FreezeAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken: cancellationToken);
        
        if (account == null)
            return Result<Guid>.Failure("Account not found");

        account.Freeze(request.Reason, "system");
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Guid>.Success(account.Id);
    }
}

// =============== Apply Interest Command ===============
public record ApplyInterestCommand(Guid AccountId) : IRequest<Result<Guid>>;

public class ApplyInterestCommandHandler : IRequestHandler<ApplyInterestCommand, Result<Guid>>
{
    private readonly AccountsDbContext _context;

    public ApplyInterestCommandHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(ApplyInterestCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken: cancellationToken);
        
        if (account == null)
            return Result<Guid>.Failure("Account not found");

        account.ApplyInterest("system");
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Guid>.Success(account.Id);
    }
}
