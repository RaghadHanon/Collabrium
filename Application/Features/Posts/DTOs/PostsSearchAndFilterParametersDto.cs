using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Posts.DTOs;

public sealed class PostsSearchAndFilterParametersDto
    : QueryParametersDto<PostsCursorDto?>
{
    public string? TenantId { get; set; }

    public string? Search { get; set; }

    public string? CreatedByUserId { get; set; }

    public PostType? Type { get; set; }

    public bool? IsPinned { get; set; }

    public List<string>? TagIdsAny { get; set; }
    public List<string>? TagIdsAll { get; set; }

    public bool? HasImages { get; set; }
    public bool? HasLinks { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }

    public string? CurrentUserId { get; set; }
}