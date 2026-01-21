namespace API.Contracts.Tenants;

public class GetTenantsRequest
{
    public string? NameContains { get; set; }
    public string? DomainContains { get; set; }
    public string? DescriptionContains { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
