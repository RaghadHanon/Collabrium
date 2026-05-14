using Application.Common;
using Application.Features.RoadmapResources.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapResources.Commands;

public sealed class UpdateResource
{
    public sealed class Command : IRequest<Result<ResourceDto>>
    {
        public required string RoadmapId { get; set; }
        public required string TopicNodeId { get; set; }
        public required string SubTopicId { get; set; }
        public required string ResourceId { get; set; }

        public string? Title { get; set; }
        public string? Url { get; set; }
        public RoadmapResourceType? Type { get; set; }
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

            var resource = await resourcesRepository.GetByIdAsync(
                tenantId: tenantId,
                roadmapId: request.RoadmapId,
                topicNodeId: request.TopicNodeId,
                subTopicId: request.SubTopicId,
                resourceId: request.ResourceId,
                ct: cancellationToken);

            if (resource is null)
                return Result<ResourceDto>.Failure("Resource not found.", HttpStatusCode.NotFound);

            var titleDecision = PatchHelpers.DecideStringPatch(request.Title);
            if (titleDecision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var newTitle = titleDecision.Value!;
                if (string.IsNullOrWhiteSpace(newTitle))
                    return Result<ResourceDto>.Failure("Title cannot be empty.", HttpStatusCode.BadRequest);
            }

            var urlDecision = PatchHelpers.DecideStringPatch(request.Url);
            if (urlDecision.Action == PatchHelpers.PatchStringAction.Set)
            {
                var newUrl = urlDecision.Value!;
                if (string.IsNullOrWhiteSpace(newUrl))
                    return Result<ResourceDto>.Failure("Url cannot be empty.", HttpStatusCode.BadRequest);
            }

            var newTitleValue =
                PatchHelpers.ApplyStringPatch(resource.Title, request.Title) ?? resource.Title;

            var newUrlValue =
                PatchHelpers.ApplyStringPatch(resource.Url, request.Url) ?? resource.Url;

            var newType = request.Type ?? resource.Type;

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                resource.UpdateDetails(
                    title: newTitleValue,
                    url: newUrlValue,
                    type: newType);

                await resourcesRepository.UpdateAsync(resource, ct);
            }, cancellationToken);

            var dto = await resourcesReadModel.GetByIdAsync(
                tenantId: tenantId,
                roadmapId: request.RoadmapId,
                topicNodeId: request.TopicNodeId,
                subTopicId: request.SubTopicId,
                resourceId: request.ResourceId,
                ct: cancellationToken);

            return dto is null
                ? Result<ResourceDto>.Failure("Resource updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<ResourceDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
