using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Sprints.Commands;

public class DeleteSprint
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
        public required string SprintId { get; set; }
    }

    public class Handler(
        ISprintsRepository sprintsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var sprint = await sprintsRepository.GetByIdAsync(tenantId, request.SprintId, request.ProjectId, cancellationToken);
            if (sprint is null)
                return Result<Unit>.Failure("Sprint not found.", HttpStatusCode.NotFound);

            var hasItems = await sprintsRepository.HasWorkItemsAsync(tenantId, request.SprintId, cancellationToken);
            if (hasItems)
                return Result<Unit>.Failure("Cannot delete a sprint that has work items.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async trxCt =>
            {
                await sprintsRepository.RemoveAsync(tenantId, request.SprintId, trxCt);
                await unitOfWork.SaveChangesAsync(trxCt);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
