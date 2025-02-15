namespace Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public long Version { get; set; } = 1;

    public bool IsDeleted { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
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
        }
    }
}