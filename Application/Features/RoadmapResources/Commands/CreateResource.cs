using Application.Common;
using Application.Features.RoadmapResources.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapResources.Commands
{
    public sealed class CreateResource
    {
        public sealed class Command : IRequest<Result<ResourceDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }

            public required string Title { get; set; }
            public required string Url { get; set; }
            public RoadmapResourceType Type { get; set; }
            public int Order { get; set; }
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

                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<ResourceDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var subTopic = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);
                var isOrderUsed = await resourcesRepository.IsOrderUsedAsync(
                    subTopicId: request.SubTopicId,
                    order: request.Order,
                    ct: cancellationToken);

                if (isOrderUsed)
                {
                    return Result<ResourceDto>.Failure(
                        "Order already exists in this subtopic.",
                        HttpStatusCode.BadRequest);
                }

                if (subTopic is null)
                    return Result<ResourceDto>.Failure("Subtopic not found.", HttpStatusCode.NotFound);

                RoadmapResource resource = null!;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    resource = new RoadmapResource(
                        subTopicId: request.SubTopicId,
                        title: request.Title,
                        url: request.Url,
                        type: request.Type,
                        order: request.Order);

                    await resourcesRepository.AddAsync(resource, ct);
                }, cancellationToken);

                var dto = await resourcesReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    resourceId: resource.Id,
                    ct: cancellationToken);

                return dto is null
                    ? Result<ResourceDto>.Failure("Resource created but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ResourceDto>.Success(dto, HttpStatusCode.Created);
            }
        }
    }
}
