using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands
{
    public sealed class PublishRoadmap
    {
        public sealed class Command : IRequest<Result<RoadmapDto>>
        {
            public required string RoadmapId { get; set; }
        }

        public sealed class Handler(
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
                    return Result<RoadmapDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound
                    );
                var roadmap = await roadmapsRepository.GetByIdAsync(request.RoadmapId, tenantId, cancellationToken);
                if (roadmap == null)
                    return Result<RoadmapDto>.Failure("Roadmap not found.", HttpStatusCode.NotFound);

                roadmap.Publish(DateTime.UtcNow);
                roadmap.Show(); // IsVisible = true

                await unitOfWork.SaveChangesAsync(cancellationToken);

                var dto = await roadmapsReadModel.GetByIdAsync(roadmap.TenantId, roadmap.Id, cancellationToken);
                return dto is null
                    ? Result<RoadmapDto>.Failure("Roadmap published but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<RoadmapDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}

