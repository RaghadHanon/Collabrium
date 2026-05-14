namespace Infrastructure.Persistence.Entities;

public class SubTopicEntity : DbEntity
{
    public string TopicNodeId { get; set; } = null!;
    public TopicNodeEntity TopicNode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
    public ICollection<ResourceEntity> Resources { get; set; } = new List<ResourceEntity>();
}
