using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Commands;

public class AddTags
{
    public class Command : IRequest<Result<string>>
    {
        public required string RoadmapId;
        public required IReadOnlyCollection<string> TagIds { get; set; }

    }
    public class Handler(
   IRoadmapsRepository roadmapsRepository,
   ITenantContext tenantContext,
   IUnitOfWork unitOfWork
) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {

            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            try
            {
                await roadmapsRepository.AddTagsAsync(tenantId, request.RoadmapId, request.TagIds, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<string>.Success("tags added successfully.", HttpStatusCode.OK);
            }
            catch (InvalidOperationException ex)
            {
                return Result<string>.Failure(ex.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}
