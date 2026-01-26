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
        ITenantRepository tenantRepository
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Current tenant not found.", HttpStatusCode.NotFound);

            var uploadResult = await photoService.UploadPhoto(request.File);
            if (uploadResult == null)
                return Result<Unit>.Failure("Failed to upload the photo.", HttpStatusCode.BadRequest);

            var updated = await tenantRepository.UpdateProfileImageAsync(
                tenantId,
                uploadResult.Url,
                uploadResult.PublicId,
                cancellationToken);

            if (!updated)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            await tenantRepository.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}