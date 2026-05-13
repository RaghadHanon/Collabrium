using Application.Common.DTOs;
using Domain.Enums;

namespace Application.Features.ProjectsHistories.DTOs;

public class ProjectHistorySearchAndFiltersParametersDto : QueryParametersDto<DateTime?>
{
    public string? MadeByUserId { get; set; }
    public string? PropertyName { get; set; }
    public ProjectHistoryScope? Scope { get; set; }
    public ActionType? ActionType { get; set; }
    public string? RelatedEntityTitle { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}