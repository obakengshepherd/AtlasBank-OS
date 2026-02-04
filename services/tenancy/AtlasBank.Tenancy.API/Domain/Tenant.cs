using AtlasBank.Core.Domain.Common;

namespace AtlasBank.Tenancy.API.Domain;

public class Tenant : AggregateRoot
{
    public string TenantCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string LegalName { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PrimaryContact { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public Dictionary<string, string> Settings { get; private set; } = new();

    private Tenant() { }

    public static Tenant Create(
        string tenantCode,
        string name,
        string legalName,
        string registrationNumber,
        string country,
        string primaryContact,
        string email,
        string phone,
        string createdBy)
    {
        var tenant = new Tenant
        {
            TenantCode = tenantCode.ToUpperInvariant(),
            Name = name,
            LegalName = legalName,
            RegistrationNumber = registrationNumber,
            Country = country,
            PrimaryContact = primaryContact,
            Email = email.ToLowerInvariant(),
            Phone = phone,
            Status = TenantStatus.Pending,
            CreatedBy = createdBy
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.TenantCode, tenant.Name));
        return tenant;
    }

    public void Activate(string activatedBy)
    {
        if (Status != TenantStatus.Pending)
            throw new InvalidOperationException("Only pending tenants can be activated");

        Status = TenantStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        UpdateTimestamp(activatedBy);
        AddDomainEvent(new TenantActivatedEvent(Id, TenantCode));
    }

    public void Suspend(string reason, string suspendedBy)
    {
        if (Status != TenantStatus.Active)
            throw new InvalidOperationException("Only active tenants can be suspended");

        Status = TenantStatus.Suspended;
        UpdateTimestamp(suspendedBy);
        AddDomainEvent(new TenantSuspendedEvent(Id, TenantCode, reason));
    }

    public void UpdateSettings(Dictionary<string, string> newSettings, string updatedBy)
    {
        Settings = newSettings;
        UpdateTimestamp(updatedBy);
    }

    public void UpdateContactInfo(string email, string phone, string updatedBy)
    {
        Email = email.ToLowerInvariant();
        Phone = phone;
        UpdateTimestamp(updatedBy);
    }
}

public enum TenantStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Deactivated = 3
}

// Domain Events
public record TenantCreatedEvent(Guid TenantId, string TenantCode, string Name) : DomainEvent;
public record TenantActivatedEvent(Guid TenantId, string TenantCode) : DomainEvent;
public record TenantSuspendedEvent(Guid TenantId, string TenantCode, string Reason) : DomainEvent;
