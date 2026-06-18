using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Courses.DTOs;

public sealed class CoursesSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? Search { get; set; }
    public string? Provider { get; set; }
    public string? InstructorName { get; set; }

    public CourseLevel? Level { get; set; }
    public CourseLanguage? Language { get; set; }
    public string? TenantId { get; set; }

    public IReadOnlyCollection<string>? TagIdsAny { get; set; }

    public IReadOnlyCollection<string>? TagIdsAll { get; set; }
    public decimal? MinEstimatedDurationInHours { get; set; }
    public decimal? MaxEstimatedDurationInHours { get; set; }

    public bool? HasThumbnail { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }

}
