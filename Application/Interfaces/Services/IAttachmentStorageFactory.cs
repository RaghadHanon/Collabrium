using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IAttachmentStorageFactory
{
    IAttachmentStorage Get(AttachmentType type);
}