using Application.Common;
using Application.Features.RoadmapNodes.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.RoadmapNodes.Commands;

public sealed class MoveRoadmapNodeDown
{
    public sealed class Command : IRequest<Result<RoadmapNodeDto>>
    {
        public required string RoadmapId { get; set; }
        public required string NodeId { get; set; }
    }

    public sealed class Handler(
        IRoadmapNodesRepository roadmapNodesRepository,
        IRoadmapNodesReadModel roadmapNodesReadModel,
        IRoadmapsRepository roadmapsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<RoadmapNodeDto>>
    {
        public async Task<Result<RoadmapNodeDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapNodeDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var roadmap = await roadmapsRepository.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            if (roadmap is null)
                return Result<RoadmapNodeDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

            var nodes = await roadmapNodesRepository.GetByRoadmapIdAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            var current = nodes.FirstOrDefault(n => n.Id == request.NodeId);
            if (current is null)
                return Result<RoadmapNodeDto>.Failure("Node not found.", HttpStatusCode.NotFound);

                var next = nodes
                    .Where(n => n.Order > current.Order)
                    .OrderBy(n => n.Order)
                    .FirstOrDefault();

                if (next is null)
                {
                    var dto = await roadmapNodesReadModel.GetByIdAsync(
                        tenantId,
                        request.RoadmapId,
                        current.Id,
                        cancellationToken);

                return dto is null
                    ? Result<RoadmapNodeDto>.Failure("Node could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<RoadmapNodeDto>.Success(dto, HttpStatusCode.OK);
            }

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    await roadmapNodesRepository.SwapOrderAsync(
                        tenantId: tenantId,
                        roadmapId: request.RoadmapId,
                        firstNodeId: current.Id,
                        secondNodeId: next.Id,
                        ct: ct);
                }, cancellationToken);

            var updatedDto = await roadmapNodesReadModel.GetByIdAsync(
                tenantId,
                request.RoadmapId,
                current.Id,
                cancellationToken);

            return updatedDto is null
                ? Result<RoadmapNodeDto>.Failure("Move down succeeded but reload failed.", HttpStatusCode.InternalServerError)
                : Result<RoadmapNodeDto>.Success(updatedDto, HttpStatusCode.OK);
        }
    }
}
