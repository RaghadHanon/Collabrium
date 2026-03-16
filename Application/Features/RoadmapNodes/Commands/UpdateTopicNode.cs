using Application.Common;
using Application.Features.RoadmapNodes.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapNodes.Commands
{
    public sealed class UpdateTopicNode
    {
        public sealed class Command : IRequest<Result<TopicNodeDto>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }

            public string? Title { get; set; }
            public string? Description { get; set; }
            public int? Order { get; set; }
        }

        public sealed class Handler(
            IRoadmapNodesRepository roadmapNodesRepository,
            IRoadmapNodesReadModel roadmapNodesReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<TopicNodeDto>>
        {
            public async Task<Result<TopicNodeDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<TopicNodeDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);


                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<TopicNodeDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var node = await roadmapNodesRepository.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    nodeId: request.TopicNodeId,
                    ct: cancellationToken);

                if (node is null)
                    return Result<TopicNodeDto>.Failure("Topic node not found.", HttpStatusCode.NotFound);

                if (node is not Domain.Models.TopicNode topic)
                    return Result<TopicNodeDto>.Failure("Node is not a topic.", HttpStatusCode.BadRequest);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    topic.Update(
                        title: request.Title,
                        description: request.Description,
                        order: request.Order);

                    await roadmapNodesRepository.UpdateAsync(topic, ct);
                }, cancellationToken);

                var dto = await roadmapNodesReadModel.GetTopicAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: topic.Id,
                    ct: cancellationToken);

                return dto is null
                    ? Result<TopicNodeDto>.Failure("Topic node updated but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<TopicNodeDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
