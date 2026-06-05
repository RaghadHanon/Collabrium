using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record ToggleTeamActivationEvent(
    string TenantId,
    string TeamId,
    List<UserSnapshotDto> TeamMembers,
    string AssignerUserId,
    string AssignerFullName,
    bool IsActivated,
    string TeamName
) : INotification;