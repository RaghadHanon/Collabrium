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
public sealed class CreateEvent
{
    public sealed class Command : IRequest<Result<EventDto>>
    {
        public required string Title { get; set; }
        public string? Description { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public bool IsOnline { get; set; }

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
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<EventDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            Event ev = null!;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                ev = new Event(
                    tenantId: tenantId,
                    title: request.Title,
                    startAt: request.StartAt,
                    endAt: request.EndAt,
                    isOnline: request.IsOnline,
                    createdByUserId: currentUserId,
                    description: request.Description,
                    location: request.Location,
                    meetingUrl: request.MeetingUrl,
                    maxAttendees: request.MaxAttendees);

                await eventsRepository.AddAsync(ev, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await eventsReadModel.GetByIdAsync(
                tenantId,
                ev.Id,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Event created but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<EventDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}