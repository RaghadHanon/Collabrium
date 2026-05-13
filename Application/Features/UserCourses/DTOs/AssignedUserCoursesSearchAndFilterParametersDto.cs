using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.UserCourses.DTOs;

public sealed class AssignedUserCoursesSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }
    public string? AssignedByUserId { get; set; }

    public string? CourseId { get; set; }
    public string? AssignedToUserId { get; set; }

    public string? Search { get; set; }

    public UserCourseStatus? Status { get; set; }
    public bool? IsSaved { get; set; }
    public bool? IsAssigned { get; set; }

    public DateTime? AssignedAfter { get; set; }
    public DateTime? AssignedBefore { get; set; }

    public DateTime? StartedAfter { get; set; }
    public DateTime? StartedBefore { get; set; }

    public DateTime? CompletedAfter { get; set; }
    public DateTime? CompletedBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
}

