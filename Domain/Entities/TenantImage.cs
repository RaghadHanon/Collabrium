namespace Domain.Models;

public sealed class TenantImage : Image
{
    public TenantImage(string url, string publicId) : base(url, publicId) { }
}
