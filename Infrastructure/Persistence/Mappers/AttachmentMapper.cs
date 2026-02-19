using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class AttachmentMapper
{
    public static Attachment ToDomain(AttachmentEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : Attachment.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            fileName: entity.FileName,
            storagePath: entity.StoragePath,
            url: entity.Url,
            contentType: entity.ContentType,
            type: entity.Type,
            uploadedByUserId: entity.UploadedByUserId,
            uploadedAt: entity.UploadedAt
        );
    }

    public static AttachmentEntity ToEntity(Attachment domain, string workItemId)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new AttachmentEntity
            {
                Id = domain.Id,
                WorkItemId = workItemId,
                FileName = domain.FileName,
                StoragePath = domain.StoragePath,
                Url = domain.Url,
                ContentType = domain.ContentType,
                Type = domain.Type,
                UploadedByUserId = domain.UploadedByUserId,
                UploadedAt = domain.UploadedAt,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
    }

    public static void UpdateEntity(Attachment domain, AttachmentEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.FileName = domain.FileName;
        entity.StoragePath = domain.StoragePath;
        entity.Url = domain.Url;
        entity.ContentType = domain.ContentType;
        entity.Type = domain.Type;
        entity.UploadedByUserId = domain.UploadedByUserId;
        entity.UploadedAt = domain.UploadedAt;
        entity.UpdatedAt = domain.UpdatedAt;
    }
}