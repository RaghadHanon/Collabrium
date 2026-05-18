using Domain.Enums;

namespace API.Contracts.Announcements;

public sealed class GetAnnouncementsRequest
{
    public string? Search { get; set; }

    public AnnouncementPriority? Priority { get; set; }

    public AnnouncementScopeFilter Scope { get; set; }
        = AnnouncementScopeFilter.All;

    public int PageSize { get; set; } = 10;

    public DateTime? Cursor { get; set; }
}