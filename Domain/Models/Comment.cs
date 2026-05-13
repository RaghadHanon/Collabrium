using Domain.Core;

namespace Domain.Models;

public class Comment : Entity
{
    public string Content { get; private set; } = null!;
    public string CreatedByUserId { get; private set; } = null!;

    protected Comment() { }

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

    public void Edit(string content)
    {
        Content = Guard.NotNullOrWhiteSpace(content, nameof(content));
        Touch();
    }
}