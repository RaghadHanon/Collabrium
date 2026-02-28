namespace API.Contracts.WorkItems.Attachments;

public class AddWorkItemAttachmentRequest
{
    public required string Type { get; set; }
    public required IFormFile File { get; set; }
}
