using Application.Common.DTOs;
using Application.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Enums;
using Infrastructure.FilesStorage.Factory;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.FilesStorage;

public sealed class CloudinaryVideoStorage : IAttachmentStorage
{
    private readonly ICloudinary _cloudinary;

    public CloudinaryVideoStorage(ICloudinaryClientFactory factory)
    {
        _cloudinary = factory.Create();
    }

    public async Task<StoredFileResult?> UploadAsync(
        IFormFile file,
        AttachmentType type,
        string folder,
        CancellationToken ct = default)
    {
        if (type != AttachmentType.Video)
            return null;

        if (file is null || file.Length <= 0)
            return null;

        await using var stream = file.OpenReadStream();

        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        return result.Error != null
            ? throw new Exception(result.Error.Message)
            : new StoredFileResult(
            StoragePath: result.PublicId,
            Url: result.SecureUrl.AbsoluteUri,
            ContentType: file.ContentType,
            Size: file.Length,
            FileName: file.FileName
        );
    }

    public async Task<string> DeleteAsync(
        string storagePathOrPublicId,
        AttachmentType type,
        CancellationToken ct = default)
    {
        if (type != AttachmentType.Video)
            throw new InvalidOperationException("CloudinaryVideoStorage can delete Video only.");

        if (string.IsNullOrWhiteSpace(storagePathOrPublicId))
            throw new ArgumentException("PublicId is required.", nameof(storagePathOrPublicId));

        var deletionParams = new DeletionParams(storagePathOrPublicId)
        {
            ResourceType = ResourceType.Video
        };

        var result = await _cloudinary.DestroyAsync(deletionParams);

        return result.Error != null ? throw new Exception(result.Error.Message) : result.Result;
    }
}