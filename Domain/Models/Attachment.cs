using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public sealed class Attachment : Entity
{
    public string FileName { get; private set; } = null!;
    public string StoragePath { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public AttachmentType Type { get; private set; }

    public string UploadedByUserId { get; private set; } = null!;
    public DateTime UploadedAt { get; private set; }

    private Attachment() { }

    public Attachment(
        string id,
        string fileName,
        string storagePath,
        string contentType,
        AttachmentType type,
        string uploadedByUserId) : base(id)
    {
        FileName = Guard.NotNullOrWhiteSpace(fileName, nameof(fileName)).Trim();
        StoragePath = Guard.NotNullOrWhiteSpace(storagePath, nameof(storagePath)).Trim();
        ContentType = Guard.NotNullOrWhiteSpace(contentType, nameof(contentType)).Trim();
        Type = type;

        UploadedByUserId = Guard.NotNullOrWhiteSpace(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAt = DateTime.UtcNow;
    }

    public static Attachment Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string fileName,
        string storagePath,
        string contentType,
        AttachmentType type,
        string uploadedByUserId,
        DateTime uploadedAt)
    {
        var att = new Attachment(id, fileName, storagePath, contentType, type, uploadedByUserId);
        att.UploadedAt = uploadedAt;             
        att.SetTimestamps(createdAt, updatedAt);  
        return att;
    }
}