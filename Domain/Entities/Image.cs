namespace Domain.Models;

public abstract class Image : Entity
{
    public string Url { get; private set; } = null!;
    public string PublicId { get; private set; } = null!;

    public Image() { }

    public Image(string url, string publicId)
    {
        Url = url;
        PublicId = publicId;
    }

    public void Update(string url, string publicId)
    {
        Url = url;
        PublicId = publicId;
    }
}
