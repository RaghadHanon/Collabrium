namespace Domain.Models;

public sealed class Comment : Entity
{
    public string Content { get; private set; } = null!;
    public string CreatedByUserId { get; private set; } = null!;
    public string WorkItemId { get; private set; } = null!;

    public Comment() { }

    public Comment(string id, string workItemId, string createdByUserId, string content) : base(id)
    {
        WorkItemId = workItemId;
        CreatedByUserId = createdByUserId;
        Content = content;
    }

    public void Edit(string content)
    {
        Content = content;
    }
}
