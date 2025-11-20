using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.Identity.DTOs;

public class UsersSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? TenantId { get; set; }
    public string? NameContains { get; set; } 
    public Department? Department { get; set; }
    public DateTime? CreatedAfter { get; set; } 
    public DateTime? CreatedBefore { get; set; } 
    public DateTime? LastLoginAfter { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
}
