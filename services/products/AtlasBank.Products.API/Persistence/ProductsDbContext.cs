using AtlasBank.Products.API.Domain;
using AtlasBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AtlasBank.Products.API.Persistence;

public class ProductsDbContext : BaseDbContext
{
    public DbSet<FinancialProduct> Products => Set<FinancialProduct>();
    public DbSet<Loan> Loans => Set<Loan>();

    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FinancialProduct>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ProductCode }).IsUnique();

            entity.OwnsOne(e => e.MonthlyFee, money =>
            {
                money.Property(m => m.Amount).HasColumnName("MonthlyFeeAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("MonthlyFeeCurrency").HasMaxLength(3);
            });

            entity.Property(e => e.Features)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new());
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LoanNumber).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.CustomerId });

            entity.OwnsOne(e => e.PrincipalAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("PrincipalAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("PrincipalCurrency").HasMaxLength(3);
            });

            entity.OwnsOne(e => e.OutstandingBalance, money =>
            {
                money.Property(m => m.Amount).HasColumnName("OutstandingAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("OutstandingCurrency").HasMaxLength(3);
            });

            entity.OwnsOne(e => e.MonthlyPayment, money =>
            {
                money.Property(m => m.Amount).HasColumnName("MonthlyPaymentAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("MonthlyPaymentCurrency").HasMaxLength(3);
            });
        });
    }
}
