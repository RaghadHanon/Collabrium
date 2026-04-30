using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands
{
    public sealed class DeleteRoadmap
    {
        public sealed class Command : IRequest<Result<string>>
        {
            public required string RoadmapId { get; set; }
        }

        public sealed class Handler(
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<string>>
        {
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(
                    tenantId,
                    request.RoadmapId,
                    cancellationToken);

                if (roadmap is null)
                    return Result<string>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var removed = false;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await roadmapsRepository.DeleteReferencingNodesAsync(
                        tenantId,
                        request.RoadmapId,
                        ct);

                    removed = await roadmapsRepository.RemoveAsync(
                        tenantId,
                        request.RoadmapId,
                        ct);
                }, cancellationToken);

                return !removed
                    ? Result<string>.Failure("Failed to delete roadmap.", HttpStatusCode.InternalServerError)
                    : Result<string>.Success("Roadmap deleted successfully.", HttpStatusCode.OK);
            }
        }
    }
}

