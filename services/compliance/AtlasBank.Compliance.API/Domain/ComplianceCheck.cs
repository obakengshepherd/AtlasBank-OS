using AtlasBank.Core.Domain.Common;
using AtlasBank.Core.Domain.Enums;

namespace AtlasBank.Compliance.API.Domain;

public class ComplianceCheck : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public ComplianceCheckType CheckType { get; private set; }
    public ComplianceStatus Status { get; private set; }
    public int RiskScore { get; private set; }
    public string? Notes { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    private ComplianceCheck() { }

    public static ComplianceCheck Create(
        string tenantId,
        string entityId,
        string entityType,
        ComplianceCheckType checkType,
        string createdBy)
    {
        var check = new ComplianceCheck
        {
            TenantId = tenantId,
            EntityId = entityId,
            EntityType = entityType,
            CheckType = checkType,
            Status = ComplianceStatus.Pending,
            RiskScore = 0,
            CreatedBy = createdBy
        };

        check.AddDomainEvent(new ComplianceCheckCreatedEvent(check.Id, entityId, checkType));
        return check;
    }

    public void Approve(string approvedBy, string? notes = null)
    {
        Status = ComplianceStatus.Approved;
        ReviewedBy = approvedBy;
        ReviewedAt = DateTime.UtcNow;
        Notes = notes;
        UpdateTimestamp(approvedBy);
        AddDomainEvent(new ComplianceCheckApprovedEvent(Id, EntityId, CheckType));
    }

    public void Reject(string rejectedBy, string reason)
    {
        Status = ComplianceStatus.Rejected;
        ReviewedBy = rejectedBy;
        ReviewedAt = DateTime.UtcNow;
        Notes = reason;
        UpdateTimestamp(rejectedBy);
        AddDomainEvent(new ComplianceCheckRejectedEvent(Id, EntityId, CheckType, reason));
    }

    public void UpdateRiskScore(int score, string updatedBy)
    {
        if (score < 0 || score > 100)
            throw new ArgumentException("Risk score must be between 0 and 100");

        RiskScore = score;
        UpdateTimestamp(updatedBy);
    }
}

public record ComplianceCheckCreatedEvent(Guid CheckId, string EntityId, ComplianceCheckType Type) : DomainEvent;
public record ComplianceCheckApprovedEvent(Guid CheckId, string EntityId, ComplianceCheckType Type) : DomainEvent;
public record ComplianceCheckRejectedEvent(Guid CheckId, string EntityId, ComplianceCheckType Type, string Reason) : DomainEvent;
