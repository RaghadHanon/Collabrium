using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Commands;

public sealed class DeleteTag
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public required string TagId { get; set; }
    }

    public sealed class Handler(
        ITagsRepository tagsRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var removed = await tagsRepository.RemoveAsync(tenantId, request.TagId, cancellationToken);
            if (!removed)
                return Result<Unit>.Failure("Tag not found or could not be removed.", HttpStatusCode.NotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}

