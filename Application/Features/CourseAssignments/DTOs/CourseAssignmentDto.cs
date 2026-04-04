namespace Application.Features.CourseAssignments.DTOs
{
    public sealed class CourseAssignmentDto
    {
        public required string Id { get; set; }

        public required string TenantId { get; set; }
        public required string CourseId { get; set; }
        public required string AssignedToUserId { get; set; }
        public required string AssignedByUserId { get; set; }
        public string? CourseTitle { get; set; }
        public string? AssignedByUserName { get; set; }
        public string? AssignedToUserName { get; set; }
        public string? CourseThumbnailUrl { get; set; }
        public DateTime AssignedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
