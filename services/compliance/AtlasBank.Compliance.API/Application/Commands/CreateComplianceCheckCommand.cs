using AtlasBank.Compliance.API.Domain;
using AtlasBank.Compliance.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Core.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AtlasBank.Compliance.API.Application.Commands;

public record CreateComplianceCheckCommand(
    string TenantId,
    string EntityId,
    string EntityType,
    ComplianceCheckType CheckType
) : IRequest<Result<Guid>>;

public class CreateComplianceCheckCommandValidator : AbstractValidator<CreateComplianceCheckCommand>
{
    public CreateComplianceCheckCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(50);
    }
}

public class CreateComplianceCheckCommandHandler : IRequestHandler<CreateComplianceCheckCommand, Result<Guid>>
{
    private readonly ComplianceDbContext _context;

    public CreateComplianceCheckCommandHandler(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateComplianceCheckCommand request, CancellationToken cancellationToken)
    {
        var check = ComplianceCheck.Create(
            request.TenantId,
            request.EntityId,
            request.EntityType,
            request.CheckType,
            "system"
        );

        _context.ComplianceChecks.Add(check);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(check.Id);
    }
}
