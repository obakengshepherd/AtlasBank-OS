namespace AtlasBank.Core.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string CreatedBy { get; protected set; } = string.Empty;
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
    }

    protected void UpdateTimestamp(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
