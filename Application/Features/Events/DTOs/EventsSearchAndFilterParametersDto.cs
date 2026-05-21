using Application.Common.DTOs;

namespace Application.Features.Events.DTOs;
public sealed class EventsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }
    public string? CurrentUserId { get; set; }

    public string? Search { get; set; }

    public bool? IsOnline { get; set; }
    public bool? IsCancelled { get; set; }

    public bool? HasAvailableSeats { get; set; }

    public DateTime? StartsAfter { get; set; }
    public DateTime? StartsBefore { get; set; }

    public DateTime? EndsAfter { get; set; }
    public DateTime? EndsBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}