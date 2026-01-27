using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
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
        IPhotoService photoService,
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

            var upload = await photoService.UploadPhoto(request.File);
            if (upload is null)
                return Result<Unit>.Failure("Failed to upload the photo.", HttpStatusCode.BadRequest);

            var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                tenant.SetProfileImage(upload.Url, upload.PublicId);

                await tenantRepository.UpdateAsync(tenant, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}