using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands;

public class UpdateRoadmapDetails
{
    public class Command : IRequest<Result<RoadmapDto>>
    {
        public required string RoadmapId { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public RoadmapLevel? Level { get; set; }
        public RoadmapPath? Path { get; set; }
        public DateTime? PublishAt { get; set; }
    }

    public class Handler(
        IRoadmapsRepository roadmapsRepository,
        IRoadmapsReadModel roadmapsReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<RoadmapDto>>
    {
        public async Task<Result<RoadmapDto>> Handle(Command request, CancellationToken cancellationToken)
        {

            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);


            var roadmap = await roadmapsRepository.GetByIdAsync(tenantId, request.RoadmapId, cancellationToken);
            if (roadmap is null)
                return Result<RoadmapDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);


            if (!string.IsNullOrWhiteSpace(request.Title) ||
                request.Description != null ||
                request.Level.HasValue)
            {
                roadmap.UpdateDetails(
                    title: request.Title ?? roadmap.Title,
                    description: request.Description ?? roadmap.Description,
                    level: request.Level
                );
            }


            if (request.Path.HasValue && request.Path.Value != roadmap.Path)
            {
                roadmap.ChangePath(request.Path.Value);
            }

            if (request.PublishAt.HasValue)
            {
                roadmap.Publish(request.PublishAt.Value);
            }


            await roadmapsRepository.UpdateAsync(roadmap, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);


            var dto = await roadmapsReadModel.GetByIdAsync(tenantId, roadmap.Id, cancellationToken);

            return dto is null
                ? Result<RoadmapDto>.Failure("Roadmap updated but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<RoadmapDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}

