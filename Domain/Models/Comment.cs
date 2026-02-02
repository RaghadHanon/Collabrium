using Domain.Common;

namespace Domain.Models;

public sealed class Comment : Entity
{
    public string Content { get; private set; } = null!;
    public string CreatedByUserId { get; private set; } = null!;

    private Comment() { }

    public Comment(string id, string createdByUserId, string content) : base(id)
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

    public void Edit(string content)
    {
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
        Touch();
    }
}