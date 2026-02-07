using AtlasBank.Accounts.API.Domain;
using AtlasBank.Accounts.API.Persistence;
using AtlasBank.Core.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Accounts.API.Application.Queries;

// =============== Get Account by ID Query ===============
public record GetAccountByIdQuery(Guid AccountId) : IRequest<Result<AccountDto>>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly AccountsDbContext _context;

    public GetAccountByIdQueryHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
            return Result<AccountDto>.Failure("Account not found");

        return Result<AccountDto>.Success(MapToDto(account));
    }

    private AccountDto MapToDto(Account account) => new(
        account.Id,
        account.AccountNumber,
        account.CustomerId,
        account.TenantId,
        account.ProductType.ToString(),
        account.Balance.Amount,
        account.AvailableBalance.Amount,
        account.Status.ToString(),
        account.InterestRate,
        account.LastInterestDate,
        account.CreatedAt,
        account.CreatedBy
    );
}

// =============== Get All Accounts Query ===============
public record GetAllAccountsQuery(string? TenantId = null, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<AccountDto>>>;

public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, Result<PagedResult<AccountDto>>>
{
    private readonly AccountsDbContext _context;

    public GetAllAccountsQueryHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AccountDto>>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Accounts.AsQueryable();

        if (!string.IsNullOrEmpty(request.TenantId))
        {
            query = query.Where(a => a.TenantId == request.TenantId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        var metadata = new PaginationMetadata
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<AccountDto>>.Success(new PagedResult<AccountDto>(dtos, metadata));
    }

    private AccountDto MapToDto(Account account) => new(
        account.Id,
        account.AccountNumber,
        account.CustomerId,
        account.TenantId,
        account.ProductType.ToString(),
        account.Balance.Amount,
        account.AvailableBalance.Amount,
        account.Status.ToString(),
        account.InterestRate,
        account.LastInterestDate,
        account.CreatedAt,
        account.CreatedBy
    );
}

// =============== Get Accounts by Customer Query ===============
public record GetAccountsByCustomerQuery(string CustomerId, string TenantId) : IRequest<Result<List<AccountDto>>>;

public class GetAccountsByCustomerQueryHandler : IRequestHandler<GetAccountsByCustomerQuery, Result<List<AccountDto>>>
{
    private readonly AccountsDbContext _context;

    public GetAccountsByCustomerQueryHandler(AccountsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AccountDto>>> Handle(GetAccountsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.Accounts
            .Where(a => a.CustomerId == request.CustomerId && a.TenantId == request.TenantId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var dtos = accounts.Select(MapToDto).ToList();
        return Result<List<AccountDto>>.Success(dtos);
    }

    private AccountDto MapToDto(Account account) => new(
        account.Id,
        account.AccountNumber,
        account.CustomerId,
        account.TenantId,
        account.ProductType.ToString(),
        account.Balance.Amount,
        account.AvailableBalance.Amount,
        account.Status.ToString(),
        account.InterestRate,
        account.LastInterestDate,
        account.CreatedAt,
        account.CreatedBy
    );
}

// =============== DTOs ===============
public record AccountDto(
    Guid Id,
    string AccountNumber,
    string CustomerId,
    string TenantId,
    string ProductType,
    decimal Balance,
    decimal AvailableBalance,
    string Status,
    decimal InterestRate,
    DateTime? LastInterestDate,
    DateTime CreatedAt,
    string CreatedBy
);
