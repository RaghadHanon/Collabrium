namespace Infrastructure.Persistence.Entities
{
    public class ReferenceNodeEntity : RoadmapNodeEntity
    {
        public string RefRoadmapId { get; set; } = null!;
        public RoadmapEntity RefRoadmap { get; set; } = null!;
    }

}
