using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.WorkItems.Commands;

public sealed class DeleteWorkItem
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string WorkItemId { get; set; }
    }

    public sealed class Handler(
        IWorkItemsRepository workItemsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var removed = false;

            await unitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                removed = await workItemsRepository.RemoveAsync(tenantId, request.ProjectId, request.WorkItemId, cancellationToken);
            }, cancellationToken);

            return removed
                ? Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent)
                : Result<Unit>.Failure("Work item not found in this project.", HttpStatusCode.NotFound);
        }
    }
}