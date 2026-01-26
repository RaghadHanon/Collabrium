using Application.Common.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;
public interface IPhotoService
{
    Task<string> DeletePhoto(string publicId);
    Task<PhotoUploadResult?> UploadPhoto(IFormFile file);
}