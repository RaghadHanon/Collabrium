using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Commands
{
    public sealed class CreateTag
    {
        public sealed class Command : IRequest<Result<TagDto>>
        {
            public required string Name { get; set; }

        }
        public sealed class Handler(
            ITagsRepository tagsRepository,
            ITagsReadModel tagsReadModel,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<Command, Result<TagDto>>
        {
            public async Task<Result<TagDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<TagDto>.Failure("Tenant not found.",
                        System.Net.HttpStatusCode.NotFound);
                var isUnique = await tagsRepository.IsNameUniqueAsync(tenantId, request.Name, cancellationToken);
                if (!isUnique)
                {
                    return Result<TagDto>.Failure(
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
                    ? Result<TagDto>.Failure(
                        "Tag created but could not be loaded.",
                        HttpStatusCode.InternalServerError
                    )
                    : Result<TagDto>.Success(
                        dto,
                        HttpStatusCode.Created
                    );
            }
        }

    }
}


