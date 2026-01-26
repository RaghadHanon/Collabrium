namespace Domain.Entities;

public record EmailMessage
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string PlainTextBody { get; init; } = string.Empty;
    public List<(string filename, byte[] content, string contentType)> Attachments { get; init; } = new();
}