namespace AtlasBank.Core.Application.Common;

/// <summary>
/// Pagination metadata for list queries
/// </summary>
public record PaginationMetadata
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public record PagedResult<T>(List<T> Items, PaginationMetadata Metadata);

/// <summary>
/// Repository interface for common CRUD operations
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work pattern for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Tenant context for multi-tenancy support
/// </summary>
public interface ITenantContext
{
    string TenantId { get; }
    string UserId { get; }
    List<string> Roles { get; }
    bool IsAuthenticated { get; }
}

/// <summary>
/// Default implementation of tenant context
/// </summary>
public class TenantContext : ITenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
}
