using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Announcements.DTOs;
public sealed class AnnouncementsSearchAndFilterParametersDto
    : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }

    public string? Search { get; set; }

    public AnnouncementPriority? Priority { get; set; }

    public AnnouncementScopeFilter Scope { get; set; } = AnnouncementScopeFilter.All;

    public bool? OnlyVisibleNow { get; set; }

    public DateTime? PublishAfter { get; set; }

    public DateTime? PublishBefore { get; set; }

    public DateTime? ExpiresAfter { get; set; }

    public DateTime? ExpiresBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }

    public DateTime? CreatedBefore { get; set; }

    public string? CurrentUserId { get; set; }

    public List<string> CurrentUserRoleNames { get; set; } = new();

    public List<string> CurrentUserTeamIds { get; set; } = new();
}