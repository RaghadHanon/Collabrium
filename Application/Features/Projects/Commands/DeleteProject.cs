using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Projects.Commands;

public class DeleteProject
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string ProjectId { get; set; }
    }

    public class Handler(
        IProjectsRepository projectsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var project = await projectsRepository.GetByIdAsync(tenantId, request.ProjectId, cancellationToken);
            if (project == null)
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            var removed = await projectsRepository.RemoveAsync(tenantId, request.ProjectId, cancellationToken);
            if (!removed)
                return Result<Unit>.Failure("Project not found.", HttpStatusCode.NotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
