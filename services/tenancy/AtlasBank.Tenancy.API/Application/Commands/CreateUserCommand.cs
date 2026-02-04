using AtlasBank.Tenancy.API.Domain;
using AtlasBank.Tenancy.API.Persistence;
using AtlasBank.Core.Application.Common;
using FluentValidation;
using MediatR;

namespace AtlasBank.Tenancy.API.Application.Commands;

public record CreateUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Phone,
    List<string> Roles
) : IRequest<Result<Guid>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly TenancyDbContext _context;

    public CreateUserCommandHandler(TenancyDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(
            request.TenantId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Roles,
            "system"
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
