using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Events
{
    public record CourseAssignedToTeamEvent(
     string CourseId,
     string CourseTitle,
     string CourseProvider,
     string AssignerFullName,
     List<UserSnapshotDto> Members
 ) : INotification;
}
