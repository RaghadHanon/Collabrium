using Application.Common;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Tenants.Commands;
public class UploadTenantProfileImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required IFormFile ProfileImage { get; set; }
    }

    public class Handler(IPhotoService photoService, ITenantContext tenantContext,
        ITenantRepository tenantRepository) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentTenant = tenantContext.CurrentTenant;
            if (currentTenant == null)
            {
                return Result<Unit>.Failure("Current tenant not found.", HttpStatusCode.NotFound);
            }

            var uploadResult = await photoService.UploadPhoto(request.ProfileImage);
            if (uploadResult == null) 
                return Result<Unit>.Failure("Failed to upload the photo", HttpStatusCode.BadRequest);

            currentTenant.profileImageURL = uploadResult.Url;
            await tenantRepository.SaveChangesAsync();

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
