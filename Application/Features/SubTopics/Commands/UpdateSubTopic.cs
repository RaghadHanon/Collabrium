using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.SubTopics.Commands;

public sealed class UpdateSubTopic
{
    public sealed class Command : IRequest<Result<SubTopicDto>>
    {
        public required string RoadmapId { get; set; }
        public required string TopicNodeId { get; set; }
        public required string SubTopicId { get; set; }

        public string? Title { get; set; }
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

            var subTopic = await subTopicsRepository.GetByIdAsync(
                topicNodeId: request.TopicNodeId,
                subTopicId: request.SubTopicId,
                ct: cancellationToken);

            if (subTopic is null)
                return Result<SubTopicDto>.Failure("SubTopic not found.", HttpStatusCode.NotFound);

            var titleDecision = PatchHelpers.DecideStringPatch(request.Title);
            if (titleDecision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var newTitle = titleDecision.Value!;
                if (string.IsNullOrWhiteSpace(newTitle))
                    return Result<SubTopicDto>.Failure("Title cannot be empty.", HttpStatusCode.BadRequest);
            }

            var newTitleValue =
                PatchHelpers.ApplyStringPatch(subTopic.Title, request.Title) ?? subTopic.Title;

            var newDescription =
                PatchHelpers.ApplyStringPatch(subTopic.Description, request.Description);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                subTopic.UpdateDetails(
                    title: newTitleValue,
                    description: newDescription);

                await subTopicsRepository.UpdateAsync(subTopic, ct);
            }, cancellationToken);

            var dto = await subTopicsReadModel.GetByIdAsync(
                tenantId: tenantId,
                roadmapId: request.RoadmapId,
                topicNodeId: request.TopicNodeId,
                subTopicId: request.SubTopicId,
                ct: cancellationToken);

            return dto is null
                ? Result<SubTopicDto>.Failure("SubTopic updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<SubTopicDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
