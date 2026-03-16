using Domain.Enums;

namespace API.Contracts.WorkItems.Attachments;

public class AddWorkItemAttachmentRequest
{
    public required AttachmentType Type { get; set; }
    public required IFormFile File { get; set; }
}
