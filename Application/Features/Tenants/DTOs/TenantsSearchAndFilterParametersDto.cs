using Application.Common.DTOs;

namespace Application.Features.Tenants.DTOs;
public class TenantsSearchAndFilterParametersDto : QueryParametersDto<DateTime?>
{
    public string? NameContains { get; set; }
    public string? DomainContains { get; set; }
    public string? DescriptionContains { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? PhoneNumber { get; set; }
}
