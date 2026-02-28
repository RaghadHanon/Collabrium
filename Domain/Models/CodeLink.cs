using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class CodeLink : Entity
{
    public string Url { get; private set; } = null!;
    public CodeLinkType LinkType { get; private set; }
    public string? DisplayName { get; private set; }
    public string CreatedByUserId { get; private set; }

    private CodeLink() { }

    public CodeLink(string id, string url, CodeLinkType linkType, string createdByUserId, string? displayName = null) : base(id)
    {
        Url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();
        LinkType = linkType;
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
        DisplayName = displayName?.Trim();
    }

    public CodeLink(string url, CodeLinkType linkType, string createdByUserId, string? displayName = null)
    {
        Url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();
        LinkType = linkType;
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();
        DisplayName = displayName?.Trim();
    }

    public static CodeLink Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string url,
        CodeLinkType linkType, 
        string createdByUserId,
        string? displayName)
    {
        var link = new CodeLink(id, url, linkType, createdByUserId, displayName);
        link.SetTimestamps(createdAt, updatedAt);
        return link;
    }

    public CodeLink Clone()
    {
        return Rehydrate(
            Id,
            CreatedAt,
            UpdatedAt,
            Url,
            LinkType,
            CreatedByUserId,
            DisplayName
        );
    }

    public void Update(string url, CodeLinkType linkType, string? displayName)
    {
        Url = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();
        LinkType = linkType;
        DisplayName = displayName?.Trim();
        Touch();
    }
}