using AtlasBank.Products.API.Domain;
using AtlasBank.Products.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Core.Domain.ValueObjects;
using AtlasBank.Core.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AtlasBank.Products.API.Application.Commands;

public record CreateProductCommand(
    string TenantId,
    string ProductCode,
    string ProductName,
    ProductType ProductType,
    string Description,
    decimal InterestRate,
    decimal MinimumBalance,
    decimal MaximumBalance,
    decimal MonthlyFee,
    int TermMonths,
    Dictionary<string, string> Features
) : IRequest<Result<Guid>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0).LessThanOrEqualTo(30);
        RuleFor(x => x.MinimumBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumBalance).GreaterThan(x => x.MinimumBalance);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly ProductsDbContext _context;

    public CreateProductCommandHandler(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var monthlyFee = new Money(request.MonthlyFee);

        var product = FinancialProduct.Create(
            request.TenantId,
            request.ProductCode,
            request.ProductName,
            request.ProductType,
            request.Description,
            request.InterestRate,
            request.MinimumBalance,
            request.MaximumBalance,
            monthlyFee,
            request.TermMonths,
            request.Features,
            "system"
        );

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}
