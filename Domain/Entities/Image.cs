namespace Domain.Entities;

public class Image : Entity
{
    public required string Url { get; set; }
    public required string PublicId { get; set; }
}