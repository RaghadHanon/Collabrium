using Domain.Core;
using Domain.ValueObjects;

namespace Domain.Models;

public sealed class Article : Entity
{
    private readonly HashSet<string> _tagIds = new();

    public string TenantId { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Content { get; private set; } = null!;

    public string CreatedByUserId { get; private set; } = null!;

    public StoredImage? CoverImage { get; private set; }

    public bool IsPublished { get; private set; }

    public IReadOnlyCollection<string> TagIds => _tagIds;

    public Article() { }

    public Article(
        string tenantId,
        string title,
        string content,
        string createdByUserId,
        string? description = null,
        bool isPublished = true)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        IsPublished = isPublished;

        Touch();
    }

    public Article(
        string id,
        string tenantId,
        string title,
        string content,
        string createdByUserId,
        StoredImage? coverImage,
        string? description = null,
        bool isPublished = true) : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId));
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        CoverImage = coverImage;
        IsPublished = isPublished;
    }

    public static Article Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string title,
        string description,
        string content,
        string createdByUserId,
        StoredImage? coverImage,
        bool isPublished,
        IEnumerable<string>? tagIds)
    {
        var article = new Article(
            id: id,
            tenantId: tenantId,
            title: title,
            description: description,
            content: content,
            createdByUserId: createdByUserId,
            coverImage: coverImage,
            isPublished: isPublished);

        article.SetTimestamps(createdAt, updatedAt);

        article._tagIds.Clear();

        if (tagIds is not null)
        {
            foreach (var tagId in tagIds)
            {
                if (string.IsNullOrWhiteSpace(tagId))
                    continue;

                article._tagIds.Add(tagId.Trim());
            }
        }

        return article;
    }

    public void UpdateDetails(
        string title,
        string content,
        string? description = null,
        bool? isPublished = null)
    {
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        if (isPublished.HasValue)
            IsPublished = isPublished.Value;

        Touch();
    }

    public void Publish()
    {
        if (IsPublished)
            return;

        IsPublished = true;
        Touch();
    }

    public void UnPublish()
    {
        if (!IsPublished)
            return;

        IsPublished = false;
        Touch();
    }

    public void SetCoverImage(string url, string publicId)
    {
        CoverImage = new StoredImage(url, publicId);
        Touch();
    }

    public void RemoveCoverImage()
    {
        CoverImage = null;
        Touch();
    }

    public void AddTag(string tagId)
    {
        var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

        if (_tagIds.Add(clean))
            Touch();
    }

    public void RemoveTag(string tagId)
    {
        var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

        if (_tagIds.Remove(clean))
            Touch();
    }

    public void SetTags(IEnumerable<string>? tagIds)
    {
        _tagIds.Clear();

        if (tagIds is null)
        {
            Touch();
            return;
        }

        foreach (var tagId in tagIds)
        {
            if (string.IsNullOrWhiteSpace(tagId))
                continue;

            _tagIds.Add(tagId.Trim());
        }

        Touch();
    }
}