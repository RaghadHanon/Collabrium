using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Events.Commands;
public sealed class CancelEvent
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

            var ev = await eventsRepository.GetByIdAsync(
                tenantId,
                request.EventId,
                cancellationToken);

            if (ev is null)
                return Result<EventDto>.Failure("Event not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                ev.Cancel();

                await eventsRepository.UpdateAsync(ev, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await eventsReadModel.GetByIdAsync(
                tenantId,
                ev.Id,
                currentUserId,
                cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Event cancelled but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<EventDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}