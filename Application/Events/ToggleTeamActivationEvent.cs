using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record ToggleTeamActivationEvent(
    string TeamId,
    List<UserSnapshotDto> TeamMembers,
    string AssignerFullName,
    bool IsActivated,
    string TeamName
) : INotification;