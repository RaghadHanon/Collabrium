using Application.Features.ProjectsHistories.DTOs;
using MediatR;

namespace Application.Events;

public sealed record WorkItemDetailsChangedEvent(
string TenantId,
string ProjectId,
string WorkItemId,
string WorkItemCode,
string WorkItemTitle,
string? SprintId,
string AssignedToUserId,
string ChangedByUserId,
string ChangedByFullName,
IReadOnlyCollection<RecordedProjectHistoryChangeDto> Changes
) : INotification;