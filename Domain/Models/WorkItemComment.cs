namespace Domain.Models;

public class WorkItemComment : Comment
{
    protected WorkItemComment() { }

    public WorkItemComment(
        string createdByUserId,
        string content
    ) : base(createdByUserId, content)
    {
    }

    public WorkItemComment(
        string id,
        string createdByUserId,
        string content
    ) : base(id, createdByUserId, content)
    {
    }

    public static WorkItemComment Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string createdByUserId,
        string content)
    {
        var comment = new WorkItemComment(id, createdByUserId, content);
        comment.SetTimestamps(createdAt, updatedAt);
        return comment;
    }

    public WorkItemComment Clone()
    {
        return Rehydrate(
            Id,
            CreatedAt,
            UpdatedAt,
            CreatedByUserId,
            Content
        );
    }
}