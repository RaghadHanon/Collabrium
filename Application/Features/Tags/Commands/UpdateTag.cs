using Application.Common;
using Application.Features.Tags.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Tags.Commands
{
    public sealed class UpdateTag
    {
        public sealed class Command : IRequest<Result<TagDto>>
        {
            public required string TagId { get; set; }
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
                    return Result<TagDto>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound
                    );

                var tag = await tagsRepository.GetByIdAsync(
                    tenantId,
                    request.TagId,
                    cancellationToken
                );

                if (tag is null)
                    return Result<TagDto>.Failure(
                        "Tag not found.",
                        HttpStatusCode.NotFound
                    );

                // (اختياري) تحقق من uniqueness لو الاسم تغير
                if (tag.Name != request.Name)
                {
                    var isUnique = await tagsRepository
                        .IsNameUniqueAsync(tenantId, request.Name, cancellationToken);

                    if (!isUnique)
                    {
                        return Result<TagDto>.Failure(
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
                    ? Result<TagDto>.Failure(
                        "Tag updated but could not be loaded.",
                        HttpStatusCode.InternalServerError
                    )
                    : Result<TagDto>.Success(
                        dto,
                        HttpStatusCode.OK
                    );
            }
        }
    }
}

