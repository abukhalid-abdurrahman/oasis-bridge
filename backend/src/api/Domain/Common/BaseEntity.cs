namespace Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public long Version { get; private set; } = 1;
    public bool IsDeleted => DeletedAt.HasValue;
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public string? CreatedByIp { get; set; }
    public List<string>? UpdatedByIp { get; set; } = [];
    public string? DeletedByIp { get; set; }

    public void Update(Guid userId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
        Version++;
    }

    public void Delete(Guid userId)
    {
        if (!IsDeleted)
        {
            DeletedAt = DateTimeOffset.UtcNow;
            DeletedBy = userId;
            Status = EntityStatus.Deleted;
        }
    }
}