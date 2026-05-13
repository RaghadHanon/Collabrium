using Application.Common.DTOs;

namespace Application.Features.CourseRatings.DTOs;

public sealed class CourseRatingsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }
    public string? CourseId { get; set; }
    public string? UserId { get; set; }

    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
}
