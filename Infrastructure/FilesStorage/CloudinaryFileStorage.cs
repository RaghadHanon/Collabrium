using Application.Common.DTOs;
using Application.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Enums;
using Infrastructure.FilesStorage.Factory;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Storage;

public class CloudinaryFileStorage : IAttachmentStorage
{
    private readonly ICloudinary _cloudinary;

    public CloudinaryFileStorage(ICloudinaryClientFactory factory)
    {
        _cloudinary = factory.Create();
    }

    public async Task<StoredFileResult?> UploadAsync(
        IFormFile file, AttachmentType type, string folder, CancellationToken ct = default)
    {
        if (type == AttachmentType.Image)
            return null; 

        if (file is null || file.Length <= 0)
            return null;

        await using var stream = file.OpenReadStream();

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception(result.Error.Message);

        return new StoredFileResult(
            StoragePath: result.PublicId,
            Url: result.SecureUrl.AbsoluteUri,
            ContentType: file.ContentType,
            Size: file.Length,
            FileName: file.FileName
        );
    }

    public async Task<string> DeleteAsync(string storagePathOrPublicId, AttachmentType type, CancellationToken ct = default)
    {
        if (type == AttachmentType.Image)
            throw new InvalidOperationException("CloudinaryFileStorage can delete non-image only.");

        if (string.IsNullOrWhiteSpace(storagePathOrPublicId))
            throw new ArgumentException("PublicId is required.", nameof(storagePathOrPublicId));

        var deletionParams = new DeletionParams(storagePathOrPublicId)
        {
            ResourceType = ResourceType.Raw
        };

        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Error != null)
            throw new Exception(result.Error.Message);

        return result.Result;
    }
}