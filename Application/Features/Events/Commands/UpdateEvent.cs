using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Events.Commands;
public sealed class UpdateEvent
{
    public sealed class Command : IRequest<Result<EventDto>>
    {
        public required string EventId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public bool? IsOnline { get; set; }

        public string? Location { get; set; }
        public string? MeetingUrl { get; set; }

        public int? MaxAttendees { get; set; }
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

            var ev = await eventsRepository.GetByIdAsync(
                tenantId,
                request.EventId,
                cancellationToken);

            if (ev is null)
                return Result<EventDto>.Failure("Event not found.", HttpStatusCode.NotFound);

            var finalTitle = PatchHelpers.ApplyStringPatch(ev.Title, request.Title) ?? ev.Title;
            var finalDescription = PatchHelpers.ApplyStringPatch(ev.Description, request.Description);

            var finalStartAt = request.StartAt ?? ev.StartAt;
            var finalEndAt = request.EndAt ?? ev.EndAt;

            var finalIsOnline = request.IsOnline ?? ev.IsOnline;

            var finalLocation = PatchHelpers.ApplyStringPatch(ev.Location, request.Location);
            var finalMeetingUrl = PatchHelpers.ApplyStringPatch(ev.MeetingUrl, request.MeetingUrl);

            var finalMaxAttendees = request.MaxAttendees ?? ev.MaxAttendees;

            if (finalEndAt <= finalStartAt)
                return Result<EventDto>.Failure("EndAt must be after StartAt.", HttpStatusCode.BadRequest);

            if (finalMaxAttendees.HasValue && finalMaxAttendees.Value <= 0)
                return Result<EventDto>.Failure("MaxAttendees must be greater than 0.", HttpStatusCode.BadRequest);

            if (finalIsOnline && string.IsNullOrWhiteSpace(finalMeetingUrl))
                return Result<EventDto>.Failure("MeetingUrl is required for online events.", HttpStatusCode.BadRequest);

            if (!finalIsOnline && string.IsNullOrWhiteSpace(finalLocation))
                return Result<EventDto>.Failure("Location is required for offline events.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                ev.UpdateDetails(
                    title: finalTitle,
                    description: finalDescription,
                    startAt: finalStartAt,
                    endAt: finalEndAt,
                    isOnline: finalIsOnline,
                    location: finalLocation,
                    meetingUrl: finalMeetingUrl,
                    maxAttendees: finalMaxAttendees);

                await eventsRepository.UpdateAsync(ev, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await eventsReadModel.GetByIdAsync(
                tenantId,
                ev.Id,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Event updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<EventDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}