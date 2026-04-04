using Application.Common.DTOs;

namespace Application.Features.CourseAssignments.DTOs
{
    public sealed class CourseAssignmentsSearchParametersDto : QueryParametersDto<DateTime?>
    {
        public string? TenantId { get; set; }
        public string? CourseId { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedByUserId { get; set; }

        public DateTime? AssignedAfter { get; set; }
        public DateTime? AssignedBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }
    }
}
