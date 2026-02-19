namespace API.Contracts.WorkItems.CodeLinkes;

public class AddWorkItemCodeLinkRequest
{
    public required string Url { get; set; }
    public required string LinkType { get; set; }
    public string? DisplayName { get; set; }
}