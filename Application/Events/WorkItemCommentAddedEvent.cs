using MediatR;

namespace Application.Events;

public record WorkItemCommentAddedEvent(
    string TenantId,
    string ProjectId,
    string WorkItemId,
    string WorkItemTitle,
    string WorkItemCode,
    string? SprintId,
    string CommentId,
    string CommenterUserId,
    string CommenterFullName,
    List<string> RecipientUserIds
) : INotification;