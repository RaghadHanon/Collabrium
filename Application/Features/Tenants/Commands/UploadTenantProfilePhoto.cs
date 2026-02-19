using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class UploadTenantProfilePhoto
{
    public class Command : IRequest<Result<Unit>>
    {
        public required IFormFile File { get; set; }
    }

    public class Handler(
        IAttachmentStorageFactory storageFactory,
        ITenantContext tenantContext,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id?.Trim();
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Current tenant not found.", HttpStatusCode.NotFound);

            if (request.File is null || request.File.Length == 0)
                return Result<Unit>.Failure("No file uploaded.", HttpStatusCode.BadRequest);

            var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var storage = storageFactory.Get(AttachmentType.Image);

            var stored = await storage.UploadAsync(
                request.File,
                AttachmentType.Image,
                folder: "Collabrium/TenantProfileImages",
                ct: cancellationToken);

            if (stored is null)
                return Result<Unit>.Failure("Failed to upload the photo.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                tenant.SetProfileImage(stored.Url, stored.StoragePath);

                await tenantRepository.UpdateAsync(tenant, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}