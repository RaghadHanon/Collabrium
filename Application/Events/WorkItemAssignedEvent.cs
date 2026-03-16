using MediatR;

namespace Domain.Events;
public record WorkItemAssignedEvent(
    string ProjectId,
    string WorkItemId,
    string WorkItemTitle,
    string WorkItemCode,
    string WorkItemType,
    string WorkItemPriority,
    string AssigneeUserId,
    string AssigneeEmail,
    string AssigneeFullName,
    string AssignerFullName
) : INotification;