using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record CourseAssignedEvent(
    string TenantId,
    string CourseId,
    string CourseTitle,
    string CourseProvider,
    string AssignerUserId,
    string AssignerFullName,
    UserSnapshotDto AssignedTo
) : INotification;