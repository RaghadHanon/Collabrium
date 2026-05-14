using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Commands;

public sealed class UpdateTag
{
    public sealed class Command : IRequest<Result<TagDetailsDto>>
    {
        public required string TagId { get; set; }
        public required string Name { get; set; }
    }

    public sealed class Handler(
        ITagsRepository tagsRepository,
        ITagsReadModel tagsReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<TagDetailsDto>>
    {
        public async Task<Result<TagDetailsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<TagDetailsDto>.Failure(
                    "Tenant not found.",
                    HttpStatusCode.NotFound
                );

            var tag = await tagsRepository.GetByIdAsync(
                tenantId,
                request.TagId,
                cancellationToken
            );

            if (tag is null)
                return Result<TagDetailsDto>.Failure(
                    "Tag not found.",
                    HttpStatusCode.NotFound
                );

            if (tag.Name != request.Name)
            {
                var isUnique = await tagsRepository
                    .IsNameUniqueAsync(tenantId, request.Name, cancellationToken);

                if (!isUnique)
                {
                    return Result<TagDetailsDto>.Failure(
                        "A tag with this name already exists.",
                        HttpStatusCode.BadRequest
                    );
                }
            }

            tag.Rename(request.Name);

            await tagsRepository.UpdateAsync(tag, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = await tagsReadModel.GetByIdAsync(
                tenantId,
                tag.Id,
                cancellationToken
            );

            return dto is null
                ? Result<TagDetailsDto>.Failure(
                    "Tag updated but could not be loaded.",
                    HttpStatusCode.InternalServerError
                )
                : Result<TagDetailsDto>.Success(
                    dto,
                    HttpStatusCode.OK
                );
        }
    }
}

