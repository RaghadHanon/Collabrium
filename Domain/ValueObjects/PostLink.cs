using Domain.Core;

namespace Domain.ValueObjects;

public sealed record PostLink
{
    public string Url { get; init; } = null!;

    private PostLink() { }

    private PostLink(string url)
    {
        Url = url;
    }

    public static PostLink Create(string url)
    {
        url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();

        return !Uri.TryCreate(url, UriKind.Absolute, out _) ? throw new DomainException("Invalid post link.") : new PostLink(url);
    }
}
