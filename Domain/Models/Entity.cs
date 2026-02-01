namespace Domain.Entities;

public abstract class Entity
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected Entity() { }
    protected Entity(string id)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    protected void SetTimestamps(DateTime createdAt, DateTime? updatedAt)
    {
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}