using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Events.Commands;
public sealed class DeleteEvent
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public required string EventId { get; set; }
    }

    public sealed class Handler(
        IEventsRepository eventsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var ev = await eventsRepository.GetByIdAsync(
                tenantId,
                request.EventId,
                cancellationToken);

            if (ev is null)
                return Result<Unit>.Failure("Event not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await eventsRepository.RemoveAsync(tenantId, request.EventId, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.OK);
        }
    }
}