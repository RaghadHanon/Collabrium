using Domain.Enums;

namespace API.Contracts.Announcements;

public sealed class GetManageAnnouncementsRequest
{
    public string? Search { get; set; }

    public AnnouncementPriority? Priority { get; set; }

    public int PageSize { get; set; } = 10;

    public DateTime? Cursor { get; set; }
}