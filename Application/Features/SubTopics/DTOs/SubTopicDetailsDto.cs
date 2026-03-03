namespace Application.Features.SubTopics.DTOs
{
    public sealed class SubTopicDetailsDto
    {
        public required string Id { get; set; }
        public required string TopicNodeId { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }

        public int Order { get; set; }

        public int ResourcesCount { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        public IReadOnlyCollection<ResourceDto> Resources { get; set; }
            = Array.Empty<ResourceDto>();
    }
}

