using MediatR;

namespace Application.Events;

public record WorkItemUnassignedEvent(
    string TenantId,
    string ProjectId,
    string WorkItemId,
    string WorkItemTitle,
    string WorkItemCode,
    string WorkItemType,
    string WorkItemPriority,
    string PreviousAssigneeUserId,
    string PreviousAssigneeEmail,
    string PreviousAssigneeFullName,
    string AssignerUserId,
    string AssignerFullName
) : INotification;