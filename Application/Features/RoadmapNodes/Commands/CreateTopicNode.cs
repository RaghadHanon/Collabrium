using Application.Common;
using Application.Features.RoadmapNodes.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapNodes.Commands
{
    public sealed class CreateTopicNode
    {
        public sealed class Command : IRequest<Result<TopicNodeDto>>
        {
            public required string RoadmapId { get; set; }
            public required string Title { get; set; }
            public string? Description { get; set; }
            public required int Order { get; set; }
        }

        public class Handler(
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
                    return Result<TopicNodeDto>.Failure("Tenant context is missing.", HttpStatusCode.NotFound);
                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap is null)
                    return Result<TopicNodeDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                TopicNode topic = null!;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    topic = new TopicNode(
                        tenantId: tenantId,
                        roadmapId: request.RoadmapId,
                        title: request.Title,
                        order: request.Order,
                        description: request.Description
                    );

                    await roadmapNodesRepository.AddTopicAsync(topic, ct);
                }, cancellationToken);

                var dto = await roadmapNodesReadModel.GetTopicAsync(tenantId, request.RoadmapId, topic.Id, cancellationToken);

                return dto is null
                    ? Result<TopicNodeDto>.Failure("Topic node created but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<TopicNodeDto>.Success(dto, HttpStatusCode.Created);

            }
        }
    }
}
