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
    public class CreateReferenceNode
    {
        public class Command : IRequest<Result<ReferenceNodeDto>>
        {
            public required string RoadmapId { get; set; }
            public required string Title { get; set; }
            public string? Description { get; set; }
            public required int Order { get; set; }
            public required string RefRoadmapId { get; set; }
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
                {
                    return Result<ReferenceNodeDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                }
                var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
                if (roadmap == null)
                {
                    return Result<ReferenceNodeDto>.Failure("Roadmap not found", HttpStatusCode.NotFound);
                }
                var referenced = await roadmapsRepository.GetByIdAsync(
                   tenantId,
                   request.RefRoadmapId,
                   cancellationToken);

                if (referenced is null)
                    return Result<ReferenceNodeDto>.Failure("Referenced roadmap not found.", HttpStatusCode.NotFound);

                ReferenceNode reference = null!;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    reference = new ReferenceNode(
                        tenantId: tenantId,
                        roadmapId: request.RoadmapId,
                        title: request.Title,
                        order: request.Order,
                        refRoadmapId: request.RefRoadmapId,
                        description: request.Description
                    );

                    await roadmapNodesRepository.AddReferenceAsync(reference, ct);
                }, cancellationToken);

                var dto = await roadmapNodesReadModel.GetReferenceAsync(
                    tenantId: tenantId,
                    roadmapId: request.RoadmapId,
                    referenceNodeId: reference.Id,
                    ct: cancellationToken);

                return dto is null
                    ? Result<ReferenceNodeDto>.Failure("Reference node created but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<ReferenceNodeDto>.Success(dto, HttpStatusCode.Created);
            }

        }
    }
}

