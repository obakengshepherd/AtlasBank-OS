using AtlasBank.Tenancy.API.Domain;
using AtlasBank.Tenancy.API.Persistence;
using AtlasBank.Core.Application.Common;
using FluentValidation;
using MediatR;

namespace AtlasBank.Tenancy.API.Application.Commands;

public record CreateTenantCommand(
    string TenantCode,
    string Name,
    string LegalName,
    string RegistrationNumber,
    string Country,
    string PrimaryContact,
    string Email,
    string Phone
) : IRequest<Result<Guid>>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.TenantCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly TenancyDbContext _context;

    public CreateTenantCommandHandler(TenancyDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(
            request.TenantCode,
            request.Name,
            request.LegalName,
            request.RegistrationNumber,
            request.Country,
            request.PrimaryContact,
            request.Email,
            request.Phone,
            "system"
        );

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id);
    }
}
