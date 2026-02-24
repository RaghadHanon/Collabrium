using Domain.Enums;

namespace API.Contracts.Tenants;

public class GetTenantUsersRequest
{
    public string? NameContains { get; set; }
    public Department? Department { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? LastLoginAfter { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
