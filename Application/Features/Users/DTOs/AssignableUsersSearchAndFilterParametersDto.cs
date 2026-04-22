using Application.Common.DTOs;

namespace Application.Features.Users.DTOs
{
    public sealed class AssignableUsersSearchAndFilterParametersDto
     : QueryParametersDto<DateTime?>
    {
        public string? TenantId { get; set; }
        public string? CurrentUserId { get; set; }
        public string? Search { get; set; }
    }
}
