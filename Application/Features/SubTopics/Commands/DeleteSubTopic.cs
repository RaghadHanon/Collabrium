using Application.Common;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Commands;

public sealed class DeleteSubTopic
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string RoadmapId { get; set; }
        public required string TopicNodeId { get; set; }
        public required string SubTopicId { get; set; }
    }

    public sealed class Handler(
        IRoadmapSubTopicsRepository subTopicsRepository,
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

            var roadmap = await roadmapsRepository.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            if (roadmap is null)
                return Result<string>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

            var topicExists = await subTopicsReadModel.TopicExistsInRoadmapAsync(
                tenantId: tenantId,
                roadmapId: request.RoadmapId,
                topicNodeId: request.TopicNodeId,
                ct: cancellationToken);

            if (!topicExists)
                return Result<string>.Failure("Topic node not found.", HttpStatusCode.NotFound);

            var hasResources = await subTopicsRepository.HasResourcesAsync(
                request.SubTopicId,
                cancellationToken);

            if (hasResources)
                return Result<string>.Failure("Cannot delete subtopic that has resources.", HttpStatusCode.BadRequest);

            var removed = false;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                removed = await subTopicsRepository.RemoveAsync(
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: ct);
            }, cancellationToken);

            return !removed
                ? Result<string>.Failure("SubTopic not found.", HttpStatusCode.NotFound)
                : Result<string>.Success("SubTopic deleted successfully.", HttpStatusCode.OK);
        }
    }
}
