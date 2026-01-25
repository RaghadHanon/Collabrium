using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class UpdateTenant
{
    public class Command : IRequest<Result<Unit>>
    {
        public string? Name { get; set; }
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public string? Country { get; set; }
        public IFormFile? File { get; set; }
        public string? WebsiteURL { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
    }

    public class Handler(
        IPhotoService photoService,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            string? uploadedUrl = null;
            string? uploadedPublicId = null;

            if (request.File != null)
            {
                var upload = await photoService.UploadPhoto(request.File);
                if (upload == null)
                    return Result<Unit>.Failure("Photo upload failed.", HttpStatusCode.BadRequest);

                uploadedUrl = upload.Url;
                uploadedPublicId = upload.PublicId;
            }

            await tenantRepository.UpdateAsync(
                tenant,
                cancellationToken);

            await tenantRepository.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}