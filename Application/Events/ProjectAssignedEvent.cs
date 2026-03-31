using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record ProjectAssignedEvent(
    string ProjectId,
    string ProjectTitle,
    string ProjectKey,
    List<UserSnapshotDto> TeamMembers,
    string AssignerFullName,
    string TeamName
) : INotification;