using AtlasBank.Transactions.API.Domain;
using AtlasBank.Transactions.API.Persistence;
using AtlasBank.Core.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Transactions.API.Application.Queries;

// =============== Get Transaction by ID Query ===============
public record GetTransactionByIdQuery(Guid TransactionId) : IRequest<Result<TransactionDto>>;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDto>>
{
    private readonly TransactionsDbContext _context;

    public GetTransactionByIdQueryHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TransactionDto>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction == null)
            return Result<TransactionDto>.Failure("Transaction not found");

        return Result<TransactionDto>.Success(MapToDto(transaction));
    }

    private TransactionDto MapToDto(Transaction txn) => new(
        txn.Id,
        txn.TransactionReference,
        txn.TenantId,
        txn.Type.ToString(),
        txn.SourceAccountId,
        txn.DestinationAccountId,
        txn.Amount.Amount,
        txn.Status.ToString(),
        txn.Description,
        txn.FailureReason,
        txn.ProcessedAt,
        txn.ProcessedBy,
        txn.CreatedAt
    );
}

// =============== Get All Transactions Query ===============
public record GetAllTransactionsQuery(string? TenantId = null, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<TransactionDto>>>;

public class GetAllTransactionsQueryHandler : IRequestHandler<GetAllTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    private readonly TransactionsDbContext _context;

    public GetAllTransactionsQueryHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<TransactionDto>>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsQueryable();

        if (!string.IsNullOrEmpty(request.TenantId))
        {
            query = query.Where(t => t.TenantId == request.TenantId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
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

        return Result<PagedResult<TransactionDto>>.Success(new PagedResult<TransactionDto>(dtos, metadata));
    }

    private TransactionDto MapToDto(Transaction txn) => new(
        txn.Id,
        txn.TransactionReference,
        txn.TenantId,
        txn.Type.ToString(),
        txn.SourceAccountId,
        txn.DestinationAccountId,
        txn.Amount.Amount,
        txn.Status.ToString(),
        txn.Description,
        txn.FailureReason,
        txn.ProcessedAt,
        txn.ProcessedBy,
        txn.CreatedAt
    );
}

// =============== Get Transactions by Account Query ===============
public record GetTransactionsByAccountQuery(string AccountId, string TenantId, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedResult<TransactionDto>>>;

public class GetTransactionsByAccountQueryHandler : IRequestHandler<GetTransactionsByAccountQuery, Result<PagedResult<TransactionDto>>>
{
    private readonly TransactionsDbContext _context;

    public GetTransactionsByAccountQueryHandler(TransactionsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<TransactionDto>>> Handle(GetTransactionsByAccountQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Where(t => t.TenantId == request.TenantId && 
                   (t.SourceAccountId == request.AccountId || t.DestinationAccountId == request.AccountId));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
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

        return Result<PagedResult<TransactionDto>>.Success(new PagedResult<TransactionDto>(dtos, metadata));
    }

    private TransactionDto MapToDto(Transaction txn) => new(
        txn.Id,
        txn.TransactionReference,
        txn.TenantId,
        txn.Type.ToString(),
        txn.SourceAccountId,
        txn.DestinationAccountId,
        txn.Amount.Amount,
        txn.Status.ToString(),
        txn.Description,
        txn.FailureReason,
        txn.ProcessedAt,
        txn.ProcessedBy,
        txn.CreatedAt
    );
}

// =============== DTOs ===============
public record TransactionDto(
    Guid Id,
    string TransactionReference,
    string TenantId,
    string TransactionType,
    string SourceAccountId,
    string? DestinationAccountId,
    decimal Amount,
    string Status,
    string Description,
    string? FailureReason,
    DateTime? ProcessedAt,
    string ProcessedBy,
    DateTime CreatedAt
);
