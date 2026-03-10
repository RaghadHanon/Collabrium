using Application.Common.DTOs;

namespace Application.Features.Tags.DTOs
{
    public sealed class TagsSearchAndFilterParametersDto
         : QueryParametersDto<DateTime?>
    {
        public string? TenantId { get; set; }

        // Text filter
        public string? NameContains { get; set; }
    }
}
