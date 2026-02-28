namespace API.Contracts.WorkItems.Comments;

public class UpdateWorkItemCodeLinkRequest
{
    public string? Url { get; set; }
    public string? LinkType { get; set; }
    public string? DisplayName { get; set; }
}