using Application.Common;
using Application.Features.RoadmapResources.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapResources.Queries
{
    public sealed class GetResourcesBySubTopic
    {
        public sealed class Query : IRequest<Result<List<ResourceDto>>>
        {
            public required string RoadmapId { get; set; }
            public required string TopicNodeId { get; set; }
            public required string SubTopicId { get; set; }
        }

        public sealed class Handler(
            IRoadmapResourcesReadModel resourcesReadModel,
            IRoadmapSubTopicsReadModel subTopicsReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor
        ) : IRequestHandler<Query, Result<List<ResourceDto>>>
        {
            public async Task<Result<List<ResourceDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<List<ResourceDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var userId = currentUserAccessor.GetUserId();
                bool canViewHidden = false;

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var role = await currentUserAccessor.GetUserRole(userId, cancellationToken);

                    canViewHidden = role == UserRole.TenantAdmin
                                 || role == UserRole.HR
                                 || role == UserRole.IT
                                 || role == UserRole.TeamLead
                                 || role == UserRole.CEO
                                 ;
                }

                var roadmap = await roadmapsRepository.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                includeHidden: canViewHidden,
                cancellationToken);
                if (roadmap is null)
                    return Result<List<ResourceDto>>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var subTopic = await subTopicsReadModel.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                if (subTopic is null)
                    return Result<List<ResourceDto>>.Failure("Subtopic not found.", HttpStatusCode.NotFound);

                var list = await resourcesReadModel.GetBySubTopicIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    topicNodeId: request.TopicNodeId,
                    subTopicId: request.SubTopicId,
                    ct: cancellationToken);

                return Result<List<ResourceDto>>.Success(list, HttpStatusCode.OK);
            }
        }
    }
}
