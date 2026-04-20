using Application.Common;
using Application.Features.RoadmapResources.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapResources.Commands
{
    public sealed class MoveResourceUp
    {
        public sealed class Command : IRequest<Result<ResourceDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }
            public required string ResourceId { get; set; }
        }

        public sealed class Handler(
            IRoadmapResourcesRepository resourcesRepository,
            IRoadmapResourcesReadModel resourcesReadModel,
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<ResourceDto>>
        {
            public async Task<Result<ResourceDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<ResourceDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(
                    tenantId,
                    request.RoadmapId,
                    cancellationToken);

                if (roadmap is null)
                    return Result<ResourceDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var subTopic = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                if (subTopic is null)
                    return Result<ResourceDto>.Failure("Subtopic not found.", HttpStatusCode.NotFound);

                var resources = await resourcesRepository.GetBySubTopicIdAsync(
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                var current = resources.FirstOrDefault(r => r.Id == request.ResourceId);
                if (current is null)
                    return Result<ResourceDto>.Failure("Resource not found.", HttpStatusCode.NotFound);

                if (current.Order == 1)
                {
                    var dto = await resourcesReadModel.GetByIdAsync(
                        tenantId: tenantId,
                        roadmapId: request.RoadmapId,
                        topicNodeId: request.TopicNodeId,
                        subTopicId: request.SubTopicId,
                        resourceId: current.Id,
                        ct: cancellationToken);

                    return dto is null
                        ? Result<ResourceDto>.Failure("Resource could not be loaded.", HttpStatusCode.InternalServerError)
                        : Result<ResourceDto>.Success(dto, HttpStatusCode.OK);
                }

                var previous = resources.FirstOrDefault(r => r.Order == current.Order - 1);
                if (previous is null)
                    return Result<ResourceDto>.Failure("Previous resource not found.", HttpStatusCode.InternalServerError);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await resourcesRepository.SwapOrderAsync(
                         request.SubTopicId,
                         previous.Id,
                         current.Id,
                         ct);
                }, cancellationToken);

                var updatedDto = await resourcesReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    resourceId: current.Id,
                    ct: cancellationToken);

                return updatedDto is null
                    ? Result<ResourceDto>.Failure("Resource moved up but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ResourceDto>.Success(updatedDto, HttpStatusCode.OK);
            }
        }
    }
}
