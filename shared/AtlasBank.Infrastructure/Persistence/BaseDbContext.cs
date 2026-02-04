using AtlasBank.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AtlasBank.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    public BaseDbContext(DbContextOptions options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish domain events before saving
        var domainEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after saving
        foreach (var entity in ChangeTracker.Entries<AggregateRoot>().Select(e => e.Entity))
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetIsDeletedFilter(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression GetIsDeletedFilter(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var prop = Expression.Property(param, nameof(Entity.IsDeleted));
        var condition = Expression.Equal(prop, Expression.Constant(false));
        return Expression.Lambda(condition, param);
    }
}
