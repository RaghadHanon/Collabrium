using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class AttachmentEntity : DbEntity
{
    public string WorkItemId { get; set; } = null!;
    public WorkItemEntity WorkItem { get; set; } = null!;

    public string FileName { get; set; } = null!;
    public string StoragePath { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public AttachmentType Type { get; set; }

    public string UploadedByUserId { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}