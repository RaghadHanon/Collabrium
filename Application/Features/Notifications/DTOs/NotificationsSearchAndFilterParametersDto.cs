using Application.Common.DTOs;

namespace Application.Features.Notifications.DTOs;

public sealed class NotificationsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public bool? IsRead { get; set; } 
}