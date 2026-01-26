using Application.Common.DTOs;
using Application.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Images;

public class PhotoService : IPhotoService
{
    private readonly ICloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
         );

        _cloudinary = new Cloudinary(account);

    }

    public async Task<PhotoUploadResult?> UploadPhoto(IFormFile file)
    {
        if (file.Length > 0)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill"),
                Folder = "Collabrium",
            };
            var uploadResults = await _cloudinary.UploadAsync(uploadParams);

            return uploadResults.Error != null
                ? throw new Exception(uploadResults.Error.Message)
                : new PhotoUploadResult
                {
                    PublicId = uploadResults.PublicId,
                    Url = uploadResults.SecureUrl.AbsoluteUri
                };
        }

        return null;
    }

    public async Task<string> DeletePhoto(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);

        var deletionResults = await _cloudinary.DestroyAsync(deletionParams);

        return deletionResults.Error != null ? throw new Exception(deletionResults.Error.Message) : deletionResults.Result;
    }
}