using Application.Common.DTOs;
using Domain.Enums;
using Domain.Models;

namespace Application.Features.ProjectHistories.DTOs;

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