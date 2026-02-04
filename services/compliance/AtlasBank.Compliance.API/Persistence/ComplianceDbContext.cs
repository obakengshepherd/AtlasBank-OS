using AtlasBank.Compliance.API.Domain;
using AtlasBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasBank.Compliance.API.Persistence;

public class ComplianceDbContext : BaseDbContext
{
    public DbSet<ComplianceCheck> ComplianceChecks => Set<ComplianceCheck>();

    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ComplianceCheck>(entity =>
        {
            entity.ToTable("ComplianceChecks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.EntityId, e.CheckType });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Notes).HasMaxLength(2000);
        });
    }
}
