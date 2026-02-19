using CloudinaryDotNet;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.FilesStorage.Factory;

public class CloudinaryClientFactory : ICloudinaryClientFactory
{
    private readonly CloudinarySettings _settings;

    public CloudinaryClientFactory(IOptions<CloudinarySettings> options)
    {
        _settings = options.Value;
    }

    public ICloudinary Create()
    {
        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        return new Cloudinary(account);
    }
}