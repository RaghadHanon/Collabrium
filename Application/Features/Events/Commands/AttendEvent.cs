using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Events.Commands;
public sealed class AttendEvent
{
    public sealed class Command : IRequest<Result<EventDto>>
    {
        public required string EventId { get; set; }
    }

    public sealed class Handler(
        IEventsRepository eventsRepository,
        IEventsReadModel eventsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<EventDto>>
    {
        public async Task<Result<EventDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<EventDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<EventDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var ev = await eventsRepository.GetByIdAsync(
                tenantId,
                request.EventId,
                cancellationToken);

            if (ev is null)
                return Result<EventDto>.Failure("Event not found.", HttpStatusCode.NotFound);

            if (ev.IsCancelled)
                return Result<EventDto>.Failure("Cannot attend a cancelled event.", HttpStatusCode.BadRequest);

            if (ev.IsFinished(DateTime.UtcNow))
                return Result<EventDto>.Failure("Cannot attend a finished event.", HttpStatusCode.BadRequest);

            var alreadyAttending = await eventsRepository.HasAttendanceAsync(
                tenantId,
                request.EventId,
                currentUserId,
                cancellationToken);

            if (alreadyAttending)
                return Result<EventDto>.Failure("You are already attending this event.", HttpStatusCode.BadRequest);

            var attendeesCount = await eventsRepository.GetAttendeesCountAsync(
                tenantId,
                request.EventId,
                cancellationToken);

            if (ev.MaxAttendees.HasValue && attendeesCount >= ev.MaxAttendees.Value)
                return Result<EventDto>.Failure("Event is full.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var attendance = new EventAttendance(
                    tenantId: tenantId,
                    eventId: request.EventId,
                    userId: currentUserId);

                await eventsRepository.AddAttendanceAsync(attendance, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await eventsReadModel.GetByIdAsync(
                tenantId,
                request.EventId,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Attendance saved but event could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<EventDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}