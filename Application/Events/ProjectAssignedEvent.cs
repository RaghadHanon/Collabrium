using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record ProjectAssignedEvent(
    string TenantId,
    string ProjectId,
    string ProjectTitle,
    string ProjectKey,
    List<UserSnapshotDto> TeamMembers,
    string AssignerUserId,
    string AssignerFullName,
    string TeamName
) : INotification;