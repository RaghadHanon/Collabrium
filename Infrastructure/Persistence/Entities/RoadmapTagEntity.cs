namespace Infrastructure.Persistence.Entities
{
    public class RoadmapTagEntity
    {
        public string RoadmapId { get; set; } = null!;
        public RoadmapEntity Roadmap { get; set; } = null!;

        public string TagId { get; set; } = null!;
        public TagEntity Tag { get; set; } = null!;
    }
}
