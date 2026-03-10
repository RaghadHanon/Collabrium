using Domain.Enums;

namespace Infrastructure.Persistence.Entities
{
    public class ResourceEntity : DbEntity
    {
        public string SubTopicId { get; set; } = null!;
        public SubTopicEntity SubTopic { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public RoadmapResourceType Type { get; set; }

        public int Order { get; set; }
    }
}