using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapNodes.Commands;

public sealed class DeleteRoadmapNode
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string RoadmapId { get; set; }
        public required string NodeId { get; set; }
    }

    public sealed class Handler(
        IRoadmapNodesRepository roadmapNodesRepository,
        IRoadmapNodesReadModel roadmapNodesReadModel,
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


            var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
            if (roadmap is null)
                return Result<string>.Failure("Roadmap not found.", HttpStatusCode.NotFound);
            var topic = await roadmapNodesReadModel.GetTopicDetailsAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.NodeId,
                    ct: cancellationToken);

            if (topic is not null && topic.SubTopicsCount > 0)
                return Result<string>.Failure("Cannot delete topic node that has subtopics.", HttpStatusCode.BadRequest);
            var removed = false;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                removed = await roadmapNodesRepository.RemoveAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    nodeId: request.NodeId,
                    ct: ct);
            }, cancellationToken);

            return !removed
                ? Result<string>.Failure("Node not found.", HttpStatusCode.NotFound)
                : Result<string>.Success("Node deleted successfully.", HttpStatusCode.OK);
        }
    }
}
