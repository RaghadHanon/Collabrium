using Application.Common.DTOs;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IAttachmentStorage
{
    Task<StoredFileResult?> UploadAsync(IFormFile file, AttachmentType type, string folder, CancellationToken ct = default);
    Task<string> DeleteAsync(string storagePathOrPublicId, AttachmentType type, CancellationToken ct = default);
}