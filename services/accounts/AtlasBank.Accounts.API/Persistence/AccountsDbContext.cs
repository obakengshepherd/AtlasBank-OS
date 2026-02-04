using AtlasBank.Accounts.API.Domain;
using AtlasBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Accounts.API.Persistence;

public class AccountsDbContext : BaseDbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    public AccountsDbContext(DbContextOptions<AccountsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.CustomerId });

            entity.OwnsOne(e => e.Balance, money =>
            {
                money.Property(m => m.Amount).HasColumnName("BalanceAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("BalanceCurrency").HasMaxLength(3);
            });

            entity.OwnsOne(e => e.AvailableBalance, money =>
            {
                money.Property(m => m.Amount).HasColumnName("AvailableBalanceAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("AvailableBalanceCurrency").HasMaxLength(3);
            });

            entity.Property(e => e.InterestRate).HasPrecision(5, 4);
        });
    }
}
