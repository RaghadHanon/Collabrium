using CloudinaryDotNet;

namespace Infrastructure.FilesStorage.Factory;

public interface ICloudinaryClientFactory
{
    ICloudinary Create();
}
