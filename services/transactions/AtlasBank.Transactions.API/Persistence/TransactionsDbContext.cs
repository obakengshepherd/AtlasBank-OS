using AtlasBank.Transactions.API.Domain;
using AtlasBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Transactions.API.Persistence;

public class TransactionsDbContext : BaseDbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionReference).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.SourceAccountId });
            entity.HasIndex(e => e.CreatedAt);

            entity.OwnsOne(e => e.Amount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
            });

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
        });
    }
}
