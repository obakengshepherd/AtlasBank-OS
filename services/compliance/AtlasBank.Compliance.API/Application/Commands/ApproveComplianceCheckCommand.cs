using AtlasBank.Compliance.API.Persistence;
using AtlasBank.Core.Application.Common;
using FluentValidation;
using MediatR;

namespace AtlasBank.Compliance.API.Application.Commands;

public record ApproveComplianceCheckCommand(
    Guid CheckId,
    string ApprovedBy,
    string? Notes = null
) : IRequest<Result>;

public class ApproveComplianceCheckCommandValidator : AbstractValidator<ApproveComplianceCheckCommand>
{
    public ApproveComplianceCheckCommandValidator()
    {
        RuleFor(x => x.CheckId).NotEmpty();
        RuleFor(x => x.ApprovedBy).NotEmpty();
    }
}

public class ApproveComplianceCheckCommandHandler : IRequestHandler<ApproveComplianceCheckCommand, Result>
{
    private readonly ComplianceDbContext _context;

    public ApproveComplianceCheckCommandHandler(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ApproveComplianceCheckCommand request, CancellationToken cancellationToken)
    {
        var check = await _context.ComplianceChecks.FindAsync(new object[] { request.CheckId }, cancellationToken);
        
        if (check == null)
            return Result.Failure("Compliance check not found");

        check.Approve(request.ApprovedBy, request.Notes);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
