using Domain.Enums;

namespace API.Contracts.WorkItems.CodeLinkes;

public class UpdateWorkItemCodeLinkRequest
{
    public string? Url { get; set; }
    public CodeLinkType? LinkType { get; set; }
    public string? DisplayName { get; set; }
}