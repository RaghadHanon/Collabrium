using Application.Features.Users.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Events;

public record SprintStatusChangedEvent(
    string ProjectId,
    string ProjectTitle,
    string ProjectKey,
    string SprintCode,
    string SprintName,
    SprintStatus Status,
    List<UserSnapshotDto> TeamMembers,
    string AssignerFullName
) : INotification;