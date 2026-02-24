using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands
{
    public sealed class CreateRoadmap
    {
        public sealed class Command : IRequest<Result<RoadmapDto>>
        {
            public required string Title { get; set; }
            public string? Description { get; set; }

            public RoadmapPath Path { get; set; }
            public RoadmapLevel Level { get; set; }

        }
        public sealed class Handler(
       IRoadmapsRepository roadmapsRepository,
       IRoadmapsReadModel roadmapsReadModel,
       ITenantContext tenantContext,
       ICurrentUserAccessor currentUserAccessor,
       IUnitOfWork unitOfWork
       ) : IRequestHandler<Command, Result<RoadmapDto>>
        {
            public async Task<Result<RoadmapDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<RoadmapDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound
                    );
                var createdByUserId = currentUserAccessor.GetUserId();

                if (string.IsNullOrWhiteSpace(createdByUserId))
                    return Result<RoadmapDto>.Failure(
                        "User not found.",
                        HttpStatusCode.NotFound
                    );

                var isUnique = await roadmapsRepository
                    .IsTitleUniqueAsync(tenantId, createdByUserId, cancellationToken);

                if (!isUnique)
                {
                    return Result<RoadmapDto>.Failure(
                        "A roadmap with this title already exists.",
                        HttpStatusCode.BadRequest
                    );
                }


                var roadmap = new Roadmap(
                    title: request.Title,
                    path: request.Path,
                    level: request.Level,
                    tenantId: tenantId,
                   createdByUserId: createdByUserId,
                   description: request.Description,
                   IsVisible: false

                   );

                await roadmapsRepository.AddAsync(roadmap, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var dto = await roadmapsReadModel.GetByIdAsync(
                 tenantId,
                 roadmap.Id,
                 cancellationToken
                 );

                return dto is null ?
                    Result<RoadmapDto>.Failure(
                          "Roadmap created but could not be loaded.",
                    HttpStatusCode.InternalServerError
                )
                : Result<RoadmapDto>.Success(
                    dto,
                    HttpStatusCode.Created
                );

            }

        }
    }
}
