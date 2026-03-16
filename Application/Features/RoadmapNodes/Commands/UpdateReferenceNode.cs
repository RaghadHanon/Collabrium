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
            public string? ReferenceNodeId { get; set; }

            public string? Title { get; set; }
            public string? Description { get; set; }
            public int? Order { get; set; }

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


                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
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


                var referenced = await roadmapsRepository.GetByIdAsync(tenantId, request.RefRoadmapId, cancellationToken);
                if (referenced is null)
                    return Result<ReferenceNodeDto>.Failure("Referenced roadmap not found.", HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    reference.Update(
                        title: request.Title,
                        description: request.Description,
                        order: request.Order);

                    reference.ChangeReference(request.RefRoadmapId);

                    await roadmapNodesRepository.UpdateAsync(reference, ct);
                }, cancellationToken);

                var dto = await roadmapNodesReadModel.GetReferenceAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    referenceNodeId: reference.Id,
                    ct: cancellationToken);

                return dto is null
                    ? Result<ReferenceNodeDto>.Failure("Reference node updated but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ReferenceNodeDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
