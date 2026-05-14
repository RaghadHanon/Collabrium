using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Commands;

public sealed class CreateTag
{
    public sealed class Command : IRequest<Result<TagDetailsDto>>
    {
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
                    HttpStatusCode.NotFound);

            var isUnique = await tagsRepository.IsNameUniqueAsync(
                tenantId,
                request.Name,
                cancellationToken);

            if (!isUnique)
            {
                return Result<TagDetailsDto>.Failure(
                    "A tag with this name already exists.",
                    HttpStatusCode.Conflict
                );
            }

            var tag = new Tag(
                tenantId: tenantId,
                name: request.Name
            );

            await tagsRepository.AddAsync(tag, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = await tagsReadModel.GetByIdAsync(
                tenantId,
                tag.Id,
                cancellationToken
            );

            return dto is null
                ? Result<TagDetailsDto>.Failure(
                    "Tag created but could not be loaded.",
                    HttpStatusCode.InternalServerError
                )
                : Result<TagDetailsDto>.Success(
                    dto,
                    HttpStatusCode.Created
                );
        }
    }
}


