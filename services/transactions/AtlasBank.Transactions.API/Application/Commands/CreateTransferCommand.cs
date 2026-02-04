using AtlasBank.Transactions.API.Domain;
using AtlasBank.Transactions.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Core.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace AtlasBank.Transactions.API.Application.Commands;

public record CreateTransferCommand(
    string TenantId,
    string SourceAccountId,
    string DestinationAccountId,
    decimal Amount,
    string Currency,
    string Description
) : IRequest<Result<string>>;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.DestinationAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, Result<string>>
{
    private readonly TransactionsDbContext _context;

    public CreateTransferCommandHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var money = new Money(request.Amount, request.Currency);

        var transaction = Transaction.CreateTransfer(
            request.TenantId,
            request.SourceAccountId,
            request.DestinationAccountId,
            money,
            request.Description,
            "system"
        );

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(transaction.TransactionReference);
    }
}
