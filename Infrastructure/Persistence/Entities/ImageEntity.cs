namespace Infrastructure.Persistence.Entities;

public abstract class ImageEntity : DbEntity
{
    public string Url { get; set; } = null!;
    public string PublicId { get; set; } = null!;
}
