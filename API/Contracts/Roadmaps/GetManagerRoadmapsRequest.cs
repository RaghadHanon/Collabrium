namespace API.Contracts.Roadmaps
{
    public sealed class GetManagerRoadmapsRequest : GetPublicRoadmapsRequest
    {
        public bool? IsPublished { get; set; }
        public bool? IsVisible { get; set; }
    }
}
