using Application.Common;
using Application.Features.RoadmapNodes.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapNodes.Commands
{
    public sealed class UpdateReferenceNode
    {
        public sealed class Command : IRequest<Result<ReferenceNodeDto>>
        {
            public required string RoadmapId { get; set; }
            public required string ReferenceNodeId { get; set; }

            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? RefRoadmapId { get; set; }
        }

        public sealed class Handler(
            IRoadmapNodesRepository roadmapNodesRepository,
            IRoadmapNodesReadModel roadmapNodesReadModel,
            IRoadmapsRepository roadmapsRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<ReferenceNodeDto>>
        {
            public async Task<Result<ReferenceNodeDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<ReferenceNodeDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var roadmap = await roadmapsRepository.GetByIdAsync(
                    tenantId,
                    request.RoadmapId,
                    cancellationToken);

                if (roadmap is null)
                    return Result<ReferenceNodeDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                var node = await roadmapNodesRepository.GetByIdAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    nodeId: request.ReferenceNodeId,
                    ct: cancellationToken);

                if (node is null)
                    return Result<ReferenceNodeDto>.Failure("Reference node not found.", HttpStatusCode.NotFound);

                if (node is not Domain.Models.ReferenceNode reference)
                    return Result<ReferenceNodeDto>.Failure("Node is not a reference.", HttpStatusCode.BadRequest);

                var titleDecision = PatchHelpers.DecideStringPatch(request.Title);
                if (titleDecision.Action == PatchHelpers.PatchStringAction.Set)
                {
                    var newTitle = titleDecision.Value!;
                    if (string.IsNullOrWhiteSpace(newTitle))
                        return Result<ReferenceNodeDto>.Failure("Title cannot be empty.", HttpStatusCode.BadRequest);
                }

                if (!string.IsNullOrWhiteSpace(request.RefRoadmapId))
                {
                    if (string.Equals(request.RefRoadmapId, request.RoadmapId, StringComparison.Ordinal))
                        return Result<ReferenceNodeDto>.Failure(
                            "Reference node cannot reference the same roadmap.",
                            HttpStatusCode.BadRequest);

                    var referencedRoadmap = await roadmapsRepository.GetByIdAsync(
                        tenantId,
                        request.RefRoadmapId,
                        cancellationToken);

                    if (referencedRoadmap is null)
                        return Result<ReferenceNodeDto>.Failure(
                            "Referenced roadmap not found.",
                            HttpStatusCode.NotFound);
                }

                var newTitleValue =
                    PatchHelpers.ApplyStringPatch(reference.Title, request.Title) ?? reference.Title;

                var newDescription =
                    PatchHelpers.ApplyStringPatch(reference.Description, request.Description);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    reference.UpdateDetails(
                        title: newTitleValue,
                        description: newDescription);

                    if (!string.IsNullOrWhiteSpace(request.RefRoadmapId) &&
                        !string.Equals(reference.RefRoadmapId, request.RefRoadmapId, StringComparison.Ordinal))
                    {
                        reference.ChangeReference(request.RefRoadmapId);
                    }

                    await roadmapNodesRepository.UpdateAsync(reference, ct);
                }, cancellationToken);

                var dto = await roadmapNodesReadModel.GetReferenceAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    referenceNodeId: reference.Id,
                    ct: cancellationToken);

                return dto is null
                    ? Result<ReferenceNodeDto>.Failure(
                        "Reference node updated but could not be loaded.",
                        HttpStatusCode.InternalServerError)
                    : Result<ReferenceNodeDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}


