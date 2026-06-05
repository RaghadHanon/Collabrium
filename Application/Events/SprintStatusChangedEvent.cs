using Application.Features.Users.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Events;

public record SprintStatusChangedEvent(
    string TenantId,
    string ProjectId,
    string ProjectTitle,
    string ProjectKey,
    string SprintId,
    string SprintCode,
    string SprintName,
    SprintStatus Status,
    List<UserSnapshotDto> TeamMembers,
    string AssignerUserId,
    string AssignerFullName
) : INotification;