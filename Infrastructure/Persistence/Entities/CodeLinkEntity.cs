using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class CodeLinkEntity : DbEntity
{
    public string WorkItemId { get; set; } = null!;
    public WorkItemEntity WorkItem { get; set; } = null!;

    public string Url { get; set; } = null!;
    public CodeLinkType LinkType { get; set; }
    public string? DisplayName { get; set; }
}