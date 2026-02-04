using AtlasBank.Tenancy.API.Persistence;
using AtlasBank.Core.Application.Common;
using AtlasBank.Infrastructure.Authentication;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Tenancy.API.Application.Queries;

public record AuthenticateUserQuery(
    string Email,
    string Password
) : IRequest<Result<AuthenticationResponse>>;

public record AuthenticationResponse(
    string Token,
    Guid UserId,
    string Email,
    List<string> Roles
);

public class AuthenticateUserQueryHandler : IRequestHandler<AuthenticateUserQuery, Result<AuthenticationResponse>>
{
    private readonly TenancyDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthenticateUserQueryHandler(TenancyDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null)
            return Result<AuthenticationResponse>.Failure("Invalid credentials");

        if (user.Status != Domain.UserStatus.Active)
            return Result<AuthenticationResponse>.Failure("Account is not active");

        if (!user.VerifyPassword(request.Password))
        {
            user.RecordFailedLogin();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<AuthenticationResponse>.Failure("Invalid credentials");
        }

        user.RecordSuccessfulLogin();
        await _context.SaveChangesAsync(cancellationToken);

        var token = _tokenService.GenerateToken(user.Id.ToString(), user.TenantId.ToString(), user.Roles);

        var response = new AuthenticationResponse(token, user.Id, user.Email, user.Roles);
        return Result<AuthenticationResponse>.Success(response);
    }
}
