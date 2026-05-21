using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Events.Commands;
public sealed class CancelEventAttendance
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

            var removed = false;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                removed = await eventsRepository.RemoveAttendanceAsync(
                    tenantId,
                    request.EventId,
                    currentUserId,
                    ct);

                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            if (!removed)
                return Result<EventDto>.Failure("Attendance not found.", HttpStatusCode.NotFound);

            var dto = await eventsReadModel.GetByIdAsync(
                tenantId,
                request.EventId,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Attendance cancelled but event could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<EventDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}