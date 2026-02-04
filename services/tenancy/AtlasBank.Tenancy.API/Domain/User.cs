using AtlasBank.Core.Domain.Common;

namespace AtlasBank.Tenancy.API.Domain;

public class User : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public List<string> Roles { get; private set; } = new();
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    private User() { }

    public static User Create(
        Guid tenantId,
        string email,
        string password,
        string firstName,
        string lastName,
        string phone,
        List<string> roles,
        string createdBy)
    {
        var user = new User
        {
            TenantId = tenantId,
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Roles = roles,
            Status = UserStatus.Active,
            CreatedBy = createdBy
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, email, tenantId));
        return user;
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        
        if (FailedLoginAttempts >= 5)
        {
            Status = UserStatus.Locked;
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
    }

    public void ChangePassword(string newPassword, string changedBy)
    {
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        UpdateTimestamp(changedBy);
        AddDomainEvent(new UserPasswordChangedEvent(Id, Email));
    }

    public void UpdateRoles(List<string> newRoles, string updatedBy)
    {
        Roles = newRoles;
        UpdateTimestamp(updatedBy);
    }

    public void Deactivate(string deactivatedBy)
    {
        Status = UserStatus.Deactivated;
        UpdateTimestamp(deactivatedBy);
        AddDomainEvent(new UserDeactivatedEvent(Id, Email));
    }
}

public enum UserStatus
{
    Active = 0,
    Locked = 1,
    Deactivated = 2
}

// Domain Events
public record UserCreatedEvent(Guid UserId, string Email, Guid TenantId) : DomainEvent;
public record UserPasswordChangedEvent(Guid UserId, string Email) : DomainEvent;
public record UserDeactivatedEvent(Guid UserId, string Email) : DomainEvent;
