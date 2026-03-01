using Domain.Enums;
using Domain.Models;

namespace API.Contracts.ProjectHistory;

public class GetProjectHistoryRequest
{
    public string? MadeByUserId { get; set; }
    public string? PropertyName { get; set; }
    public string? Scope { get; set; }
    public string? ActionType { get; set; }
    public string? RelatedEntityTitle { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; } = 10;
}
