using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Commands
{
    public sealed class CreateSubTopic
    {
        public sealed class Command : IRequest<Result<SubTopicDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }

            public required string Title { get; set; }
            public string? Description { get; set; }
        }

        public sealed class Handler(
            IRoadmapSubTopicsRepository subTopicsRepository,
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<SubTopicDto>>
        {
            public async Task<Result<SubTopicDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<SubTopicDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(
                    tenantId,
                    request.RoadmapId,
                    cancellationToken);

                if (roadmap is null)
                    return Result<SubTopicDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var topicExists = await subTopicsReadModel.TopicExistsInRoadmapAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    ct: cancellationToken);

                if (!topicExists)
                    return Result<SubTopicDto>.Failure("Topic node not found.", HttpStatusCode.NotFound);

                RoadmapSubTopic subTopic = null!;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var lastOrder = await subTopicsRepository.GetMaxOrderAsync(
                        topicNodeId: request.TopicNodeId,
                        ct: ct);

                    subTopic = new RoadmapSubTopic(
                        topicNodeId: request.TopicNodeId,
                        title: request.Title,
                        order: lastOrder + 1,
                        description: request.Description);

                    await subTopicsRepository.AddAsync(subTopic, ct);
                }, cancellationToken);

                var dto = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: subTopic.Id,
                    ct: cancellationToken);

                return dto is null
                    ? Result<SubTopicDto>.Failure("SubTopic created but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<SubTopicDto>.Success(dto, HttpStatusCode.Created);
            }
        }
    }
}
