using Domain.Core;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Models;

public sealed class Post : Entity
{
    private readonly HashSet<string> _tagIds = new();
    private readonly List<PostLink> _links = new();
    private readonly List<StoredImage> _images = new();

    public string TenantId { get; private set; } = null!;
    public string CreatedByUserId { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public PostType Type { get; private set; }

    public bool IsPinned { get; private set; }
    public string? PinnedByUserId { get; private set; }
    public DateTime? PinnedAt { get; private set; }

    public IReadOnlyCollection<string> TagIds => _tagIds;
    public IReadOnlyCollection<PostLink> Links => _links.AsReadOnly();
    public IReadOnlyCollection<StoredImage> Images => _images.AsReadOnly();

    private Post() { }

    public Post(
        string tenantId,
        string createdByUserId,
        string title,
        string content,
        PostType type)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();

        Type = type;
        IsPinned = false;

        Touch();
    }

    public Post(
        string id,
        string tenantId,
        string createdByUserId,
        string title,
        string content,
        PostType type) : base(id)
    {
        TenantId = Guard.NotNullOrWhiteSpace(tenantId, nameof(tenantId)).Trim();
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId)).Trim();

        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();

        Type = type;
    }

    public static Post Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string tenantId,
        string createdByUserId,
        string title,
        string content,
        PostType type,
        bool isPinned,
        string? pinnedByUserId,
        DateTime? pinnedAt,
        IEnumerable<string>? tagIds,
        IEnumerable<PostLink>? links,
        IEnumerable<StoredImage>? images)
    {
        var post = new Post(
            id,
            tenantId,
            createdByUserId,
            title,
            content,
            type);

        post.IsPinned = isPinned;
        post.PinnedByUserId = pinnedByUserId;
        post.PinnedAt = pinnedAt;

        if (tagIds is not null)
        {
            foreach (var tagId in tagIds)
            {
                if (string.IsNullOrWhiteSpace(tagId))
                    continue;

                post._tagIds.Add(tagId.Trim());
            }
        }

        if (links is not null)
        {
            foreach (var link in links.Distinct())
            {
                if (post.Type != PostType.Resource)
                    throw new DomainException("Links are allowed only for resource posts.");

                if (post._links.Count >= 5)
                    throw new DomainException("A post can have maximum 5 links.");

                post._links.Add(link);
            }
        }

        if (images is not null)
        {
            foreach (var image in images.DistinctBy(x => x.PublicId))
            {
                if (post._images.Count >= 5)
                    throw new DomainException("A post can have maximum 5 images.");

                post._images.Add(image);
            }
        }

        post.SetTimestamps(createdAt, updatedAt);

        return post;
    }

    public void UpdateDetails(
        string title,
        string content,
        PostType type)
    {
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title)).Trim();
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content)).Trim();

        Type = type;

        if (Type != PostType.Resource)
            _links.Clear();

        Touch();
    }

    public void Pin(string pinnedByUserId)
    {
        if (IsPinned)
            return;

        IsPinned = true;
        PinnedByUserId = Guard.NotNullOrWhiteSpace(pinnedByUserId, nameof(pinnedByUserId)).Trim();
        PinnedAt = DateTime.UtcNow;

        Touch();
    }

    public void Unpin()
    {
        if (!IsPinned)
            return;

        IsPinned = false;
        PinnedByUserId = null;
        PinnedAt = null;

        Touch();
    }

    public void AddTag(string tagId)
    {
        var clean = Guard.NotNullOrWhiteSpace(tagId, nameof(tagId)).Trim();

        if (_tagIds.Contains(clean))
            return;

        if (_tagIds.Count >= 5)
            throw new DomainException("A post can have maximum 5 tags.");

        _tagIds.Add(clean);
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

            if (_tagIds.Count >= 5)
                throw new DomainException("A post can have maximum 5 tags.");

            _tagIds.Add(tagId.Trim());
        }

        Touch();
    }

    public void SetLinks(IEnumerable<PostLink>? links)
    {
        _links.Clear();

        if (links is null)
        {
            Touch();
            return;
        }

        if (Type != PostType.Resource && links.Any())
            throw new DomainException("Links are allowed only for resource posts.");

        foreach (var link in links.Distinct())
        {
            if (_links.Count >= 5)
                throw new DomainException("A post can have maximum 5 links.");

            _links.Add(link);
        }

        Touch();
    }

    public void AddLink(PostLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        if (Type != PostType.Resource)
            throw new DomainException("Links are allowed only for resource posts.");

        if (_links.Contains(link))
            return;

        if (_links.Count >= 5)
            throw new DomainException("A post can have maximum 5 links.");

        _links.Add(link);
        Touch();
    }

    public void RemoveLink(string url)
    {
        var cleanUrl = Guard.NotNullOrWhiteSpace(url, nameof(url)).Trim();

        var link = _links.FirstOrDefault(x => x.Url == cleanUrl);

        if (link is null)
            return;

        _links.Remove(link);
        Touch();
    }

    public void SetImages(IEnumerable<StoredImage>? images)
    {
        _images.Clear();

        if (images is null)
        {
            Touch();
            return;
        }

        foreach (var image in images.DistinctBy(x => x.PublicId))
        {
            if (_images.Count >= 5)
                throw new DomainException("A post can have maximum 5 images.");

            _images.Add(image);
        }

        Touch();
    }

    public void AddImage(StoredImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (_images.Any(x => x.PublicId == image.PublicId))
            return;

        if (_images.Count >= 5)
            throw new DomainException("A post can have maximum 5 images.");

        _images.Add(image);
        Touch();
    }

    public void RemoveImage(string publicId)
    {
        var cleanPublicId = Guard.NotNullOrWhiteSpace(publicId, nameof(publicId)).Trim();

        var image = _images.FirstOrDefault(x => x.PublicId == cleanPublicId);

        if (image is null)
            return;

        _images.Remove(image);
        Touch();
    }
}
