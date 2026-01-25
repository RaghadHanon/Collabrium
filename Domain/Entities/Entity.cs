namespace Domain.Models;

public abstract class Entity
{
    public Entity() { }

    public Entity(string id)
    {
        Id = id;
    }

    public string Id { get; protected set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
