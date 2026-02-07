using AtlasBank.Transactions.API.Domain;
using AtlasBank.Transactions.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Core.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Transactions.API.Application.Commands;

// =============== Transfer Command ===============
public record TransferCommand(
    string TenantId,
    string SourceAccountId,
    string DestinationAccountId,
    decimal Amount,
    string Description
) : IRequest<Result<Guid>>;

public class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.DestinationAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
        RuleFor(x => x.Description).NotEmpty();
    }
}

public class TransferCommandHandler : IRequestHandler<TransferCommand, Result<Guid>>
{
    private readonly TransactionsDbContext _context;

    public TransferCommandHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(TransferCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var transferAmount = new Money(request.Amount);
            var txn = Transaction.CreateTransfer(
                request.TenantId,
                request.SourceAccountId,
                request.DestinationAccountId,
                transferAmount,
                request.Description,
                "system"
            );

            _context.Transactions.Add(txn);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<Guid>.Success(txn.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// =============== Process Transaction Command ===============
public record ProcessTransactionCommand(Guid TransactionId) : IRequest<Result<Guid>>;

public class ProcessTransactionCommandHandler : IRequestHandler<ProcessTransactionCommand, Result<Guid>>
{
    private readonly TransactionsDbContext _context;

    public ProcessTransactionCommandHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(ProcessTransactionCommand request, CancellationToken cancellationToken)
    {
        var txn = await _context.Transactions.FindAsync(new object[] { request.TransactionId }, cancellationToken: cancellationToken);
        
        if (txn == null)
            return Result<Guid>.Failure("Transaction not found");

        try
        {
            txn.Process("system");
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(txn.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// =============== Complete Transaction Command ===============
public record CompleteTransactionCommand(Guid TransactionId) : IRequest<Result<Guid>>;

public class CompleteTransactionCommandHandler : IRequestHandler<CompleteTransactionCommand, Result<Guid>>
{
    private readonly TransactionsDbContext _context;

    public CompleteTransactionCommandHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CompleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var txn = await _context.Transactions.FindAsync(new object[] { request.TransactionId }, cancellationToken: cancellationToken);
        
        if (txn == null)
            return Result<Guid>.Failure("Transaction not found");

        try
        {
            txn.Complete("system");
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(txn.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

// =============== Fail Transaction Command ===============
public record FailTransactionCommand(Guid TransactionId, string Reason) : IRequest<Result<Guid>>;

public class FailTransactionCommandValidator : AbstractValidator<FailTransactionCommand>
{
    public FailTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class FailTransactionCommandHandler : IRequestHandler<FailTransactionCommand, Result<Guid>>
{
    private readonly TransactionsDbContext _context;

    public FailTransactionCommandHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(FailTransactionCommand request, CancellationToken cancellationToken)
    {
        var txn = await _context.Transactions.FindAsync(new object[] { request.TransactionId }, cancellationToken: cancellationToken);
        
        if (txn == null)
            return Result<Guid>.Failure("Transaction not found");

        txn.Fail(request.Reason, "system");
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(txn.Id);
    }
}
