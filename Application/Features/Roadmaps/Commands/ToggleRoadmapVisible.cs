using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands
{
    public class ToggleRoadmapVisible
    {
        public class Command : IRequest<Result<RoadmapDto>>
        {
            public required string RoadmapId { get; set; }
            public required bool IsVisible { get; set; }
        }

        public class Handler(
            IRoadmapsRepository roadmapsRepository,
            IRoadmapsReadModel readModel,
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

                if (!request.IsVisible)
                    roadmap.Show();
                else
                    roadmap.Hide();

                await roadmapsRepository.UpdateAsync(roadmap, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var dto = await readModel.GetByIdAsync(tenantId, roadmap.Id, cancellationToken);
                return dto is null
                    ? Result<RoadmapDto>.Failure("Roadmap status updated but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<RoadmapDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
