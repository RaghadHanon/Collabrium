using Domain.Enums;

namespace Application.Features.Roadmaps.DTOs
{
    public sealed class RoadmapDto
    {
        public required string Id { get; set; }
        public required string TenantId { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }

        public RoadmapPath Path { get; set; }
        public RoadmapLevel Level { get; set; }

        public bool IsVisible { get; set; } = false;
        public DateTime? PublishedAt { get; set; } = null;


        public string? CreatedByUserId { get; set; }

        // لو بدك name (زي TeamLeadName) بدنا join على AspNetUsers
        public string? CreatedByUserName { get; set; }

        // مثل Count عند Team (computed field)
        public int TagsCount { get; set; }
        public int NodesCount { get; set; } // مفيد للقوائم

        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        // لو بدك تعرض IDs (اختياري بالقوائم)
        public IReadOnlyCollection<string> TagIds { get; set; } = Array.Empty<string>();
    }
}
