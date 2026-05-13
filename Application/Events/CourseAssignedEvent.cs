using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events
{
    public record CourseAssignedEvent(
      string CourseId,
      string CourseTitle,
      string CourseProvider,
      string AssignerFullName,
      UserSnapshotDto AssignedTo
  ) : INotification;
}
