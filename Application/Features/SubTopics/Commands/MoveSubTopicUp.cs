using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Commands
{
    public sealed class MoveSubTopicUp
    {
        public sealed class Command : IRequest<Result<SubTopicDto>>
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

                var subTopics = await subTopicsRepository.GetByTopicIdAsync(
                    topicNodeId: request.TopicNodeId,
                    ct: cancellationToken);

                var current = subTopics.FirstOrDefault(s => s.Id == request.SubTopicId);
                if (current is null)
                    return Result<SubTopicDto>.Failure("SubTopic not found.", HttpStatusCode.NotFound);

                if (current.Order == 1)
                {
                    var dto = await subTopicsReadModel.GetByIdAsync(
                        tenantId: tenantId,
                        roadmapId: request.RoadmapId,
                        topicNodeId: request.TopicNodeId,
                        subTopicId: current.Id,
                        ct: cancellationToken);

                    return dto is null
                        ? Result<SubTopicDto>.Failure("SubTopic could not be loaded.", HttpStatusCode.InternalServerError)
                        : Result<SubTopicDto>.Success(dto, HttpStatusCode.OK);
                }

                var previous = subTopics.FirstOrDefault(s => s.Order == current.Order - 1);
                if (previous is null)
                    return Result<SubTopicDto>.Failure("Previous subtopic not found.", HttpStatusCode.InternalServerError);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await subTopicsRepository.SwapOrderAsync(
                   request.TopicNodeId,
                   previous.Id,
                   current.Id,
                   ct);
                }, cancellationToken);

                var updatedDto = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: current.Id,
                    ct: cancellationToken);

                return updatedDto is null
                    ? Result<SubTopicDto>.Failure("SubTopic moved up but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<SubTopicDto>.Success(updatedDto, HttpStatusCode.OK);
            }
        }
    }
}
