using AtlasBank.Accounts.API.Domain;
using AtlasBank.Accounts.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Core.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AtlasBank.Accounts.API.Application.Commands;

public record CreateAccountCommand(
    string CustomerId,
    string TenantId,
    ProductType ProductType,
    decimal InterestRate
) : IRequest<Result<Guid>>;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0).LessThanOrEqualTo(20);
    }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly AccountsDbContext _context;

    public CreateAccountCommandHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = Account.Create(
            request.CustomerId,
            request.TenantId,
            request.ProductType,
            request.InterestRate,
            "system"
        );

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(account.Id);
    }
}
