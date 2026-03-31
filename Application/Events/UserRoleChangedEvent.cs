using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events;

public record UserRoleChangedEvent(
    UserSnapshotDto User,
    string OldJobTitle,
    string AssignerFullName
) : INotification;