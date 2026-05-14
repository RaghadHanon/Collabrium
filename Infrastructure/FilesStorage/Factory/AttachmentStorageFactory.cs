using Application.Interfaces.Services;
using Domain.Enums;

namespace Infrastructure.FilesStorage.Factory;

public class AttachmentStorageFactory : IAttachmentStorageFactory
{
    private readonly CloudinaryImageStorage _imageStorage;
    private readonly CloudinaryVideoStorage _videoStorage;
    private readonly CloudinaryFileStorage _fileStorage;

    public AttachmentStorageFactory(
        CloudinaryImageStorage imageStorage,
        CloudinaryVideoStorage videoStorage,
        CloudinaryFileStorage fileStorage)
    {
        _imageStorage = imageStorage;
        _videoStorage = videoStorage;
        _fileStorage = fileStorage;
    }

    public IAttachmentStorage Get(AttachmentType type)
    {
        return type switch
        {
            AttachmentType.Image => _imageStorage,
            AttachmentType.Video => _videoStorage,
            AttachmentType.Document or AttachmentType.Log => _fileStorage,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported attachment type.")
        };
    }
}