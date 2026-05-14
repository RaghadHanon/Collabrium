using Application.Common.DTOs;

namespace Application.Features.Articles.DTOs
{
    public sealed class ArticlesSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
    {
        public string? TenantId { get; set; }

        public string? Search { get; set; }

        public bool? IsPublished { get; set; }

        public string? CreatedByUserId { get; set; }

        public List<string>? TagIdsAny { get; set; }
        public List<string>? TagIdsAll { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }
    }
}
