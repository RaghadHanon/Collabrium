using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapResources.Commands
{
    public sealed class DeleteResource
    {
        public sealed class Command : IRequest<Result<string>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }
            public required string ResourceId { get; set; }
        }

        public sealed class Handler(
            IRoadmapResourcesRepository resourcesRepository,
            IRoadmapSubTopicsReadModel subTopicsReadModel,
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

                var subTopic = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                if (subTopic is null)
                    return Result<string>.Failure("Subtopic not found.", HttpStatusCode.NotFound);

                var removed = false;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    removed = await resourcesRepository.RemoveAsync(
                        tenantId: tenantId,
                        roadmapId: request.RoadmapId,
                        topicNodeId: request.TopicNodeId,
                        subTopicId: request.SubTopicId,
                        resourceId: request.ResourceId,
                        ct: ct);
                }, cancellationToken);

                return !removed
                    ? Result<string>.Failure("Resource not found.", HttpStatusCode.NotFound)
                    : Result<string>.Success("Resource deleted successfully.", HttpStatusCode.OK);
            }
        }
    }
}
