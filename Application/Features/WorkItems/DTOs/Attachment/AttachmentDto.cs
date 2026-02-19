using Domain.Enums;

namespace Application.Features.WorkItems.DTOs.Attachment;
public class AttachmentDto
{
    public required string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string FileName { get; set; }
    public required string StoragePath { get; set; }
    public required string Url { get; set; }
    public required string ContentType { get; set; }
    public AttachmentType Type { get; set; }

    public required string UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; }
}