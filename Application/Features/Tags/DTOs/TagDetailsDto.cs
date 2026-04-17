namespace Application.Features.Tags.DTOs
{
    public sealed class TagDetailsDto
    {
        public required string Id { get; set; }
        public required string TenantId { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
