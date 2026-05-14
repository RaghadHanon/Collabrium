namespace Infrastructure.Persistence.Entities;

public class TopicNodeEntity : RoadmapNodeEntity
{
    public ICollection<SubTopicEntity> SubTopics { get; set; } = new List<SubTopicEntity>();

}
