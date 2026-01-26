namespace Domain.ValueObjects;

public sealed record SocialLinks(string LinkedIn, string Facebook, string Instagram, string YouTube)
{
    public static SocialLinks Empty => new("", "", "", "");

    public static SocialLinks Create(string? linkedIn, string? facebook, string? instagram, string? youTube)
    {
        return new(linkedIn?.Trim() ?? "",
                   facebook?.Trim() ?? "",
                   instagram?.Trim() ?? "",
                   youTube?.Trim() ?? "");
    }
}