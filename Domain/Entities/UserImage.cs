namespace Domain.Models;

public sealed class UserImage : Image
{
    public UserImage(string url, string publicId) : base(url, publicId) { }
}
