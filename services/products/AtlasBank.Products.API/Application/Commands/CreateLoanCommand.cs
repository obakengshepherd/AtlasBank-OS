using AtlasBank.Products.API.Domain;
using AtlasBank.Products.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Core.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Products.API.Application.Commands;

public record CreateLoanCommand(
    string TenantId,
    string CustomerId,
    string AccountId,
    Guid ProductId,
    decimal Amount,
    int TermMonths
) : IRequest<Result<string>>;

public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
{
    public CreateLoanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.TermMonths).GreaterThan(0).LessThanOrEqualTo(360);
    }
}

public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Result<string>>
{
    private readonly ProductsDbContext _context;

    public CreateLoanCommandHandler(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
        if (product == null)
            return Result<string>.Failure("Product not found");

        var principalAmount = new Money(request.Amount);

        if (!product.IsEligible(request.Amount))
            return Result<string>.Failure("Amount does not meet product eligibility criteria");

        var loan = Loan.Create(
            request.TenantId,
            request.CustomerId,
            request.AccountId,
            request.ProductId,
            principalAmount,
            product.InterestRate,
            request.TermMonths,
            "system"
        );

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(loan.LoanNumber);
    }
}
