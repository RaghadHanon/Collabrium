using Domain.Common;

namespace Domain.Models;

public class Comment : Entity
{
    public string Content { get; private set; } = null!;
    public string CreatedByUserId { get; private set; } = null!;

    private Comment() { }

    public Comment(string id, string createdByUserId, string content) : base(id)
    {
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
    }

    public Comment(string createdByUserId, string content)
    {
        CreatedByUserId = Guard.NotNullOrWhiteSpace(createdByUserId, nameof(createdByUserId));
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
    }

    public static Comment Rehydrate(string id, DateTime createdAt, DateTime? updatedAt, string createdByUserId, string content)
    {
        var c = new Comment(id, createdByUserId, content);
        c.SetTimestamps(createdAt, updatedAt);
        return c;
    }

    public Comment Clone()
    {
        return Rehydrate(
            Id,
            CreatedAt,
            UpdatedAt,
            CreatedByUserId,
            Content
        );
    }

    public void Edit(string content)
    {
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
        Touch();
    }
}