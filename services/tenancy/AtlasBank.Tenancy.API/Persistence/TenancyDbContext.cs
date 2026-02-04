using AtlasBank.Tenancy.API.Domain;
using AtlasBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AtlasBank.Tenancy.API.Persistence;

public class TenancyDbContext : BaseDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    public TenancyDbContext(DbContextOptions<TenancyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Settings)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new());
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

            entity.Property(e => e.Roles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
        });
    }
}
