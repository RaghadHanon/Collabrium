using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.AI;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands;
public sealed class CreateRoadmapWithAi
{
    public sealed class Command : IRequest<Result<RoadmapDetailsDto>>
    {
        public RoadmapPath Path { get; set; }
        public RoadmapLevel Level { get; set; }

        public string Goal { get; set; } = string.Empty;

        public int TopicsCount { get; set; } = 5;
        public int SubTopicsPerTopic { get; set; } = 3;
    }

    public sealed class Handler(
        IRoadmapAiDraftService draftService,
        IRoadmapsRepository roadmapsRepository,
        IRoadmapNodesRepository roadmapNodesRepository,
        IRoadmapSubTopicsRepository subTopicsRepository,
        IRoadmapsReadModel roadmapsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<RoadmapDetailsDto>>
    {
        public async Task<Result<RoadmapDetailsDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var createdByUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(createdByUserId))
                return Result<RoadmapDetailsDto>.Failure("User not found.", HttpStatusCode.Unauthorized);

            var draft = await draftService.GenerateFullDraftAsync(
                request.Path,
                request.Level,
                request.Goal,
                request.TopicsCount,
                request.SubTopicsPerTopic,
                cancellationToken);

            Roadmap roadmap = null!;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                roadmap = new Roadmap(
                    title: draft.Title,
                    path: request.Path,
                    level: request.Level,
                    tenantId: tenantId,
                    createdByUserId: createdByUserId,
                    description: draft.Description
                );

                await roadmapsRepository.AddAsync(roadmap, ct);

                foreach (var topicDraft in draft.Topics.OrderBy(x => x.Order))
                {
                    var topic = new TopicNode(
                        tenantId: tenantId,
                        roadmapId: roadmap.Id,
                        title: topicDraft.Title,
                        order: topicDraft.Order,
                        description: topicDraft.Description
                    );

                    await roadmapNodesRepository.AddTopicAsync(topic, ct);

                    foreach (var subTopicDraft in topicDraft.SubTopics.OrderBy(x => x.Order))
                    {
                        var subTopic = new RoadmapSubTopic(
                            topicNodeId: topic.Id,
                            title: subTopicDraft.Title,
                            order: subTopicDraft.Order,
                            description: subTopicDraft.Description
                        );

                        await subTopicsRepository.AddAsync(subTopic, ct);
                    }
                }

                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await roadmapsReadModel.GetDetailsAsync(
                tenantId,
                roadmap.Id,
                cancellationToken);

            return dto is null
                ? Result<RoadmapDetailsDto>.Failure(
                    "Roadmap created but could not be loaded.",
                    HttpStatusCode.InternalServerError)
                : Result<RoadmapDetailsDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
