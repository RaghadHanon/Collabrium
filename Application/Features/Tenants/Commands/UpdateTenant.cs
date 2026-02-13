using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Tenants.Commands;

public class UpdateTenant
{
    public class Command : IRequest<Result<Unit>>
    {
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? WebsiteUrl { get; set; }

        public IFormFile? File { get; set; }

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
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id?.Trim();
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (request.File is not null)
                {
                    var upload = await photoService.UploadPhoto(request.File);
                    if (upload is null)
                        throw new InvalidOperationException("Photo upload failed.");

                    tenant.SetProfileImage(upload.Url, upload.PublicId);
                }

                tenant.UpdateProfile(
                    description: PatchHelpers.ApplyStringPatch(tenant.Description, request.Description),
                    country: PatchHelpers.ApplyStringPatch(tenant.Country, request.Country),
                    websiteUrl: PatchHelpers.ApplyStringPatch(tenant.WebsiteUrl, request.WebsiteUrl)
                );

                tenant.SetContactInfo(
                    email: PatchHelpers.ApplyStringPatch(tenant.ContactInfo.Email, request.Email),
                    phone: PatchHelpers.ApplyStringPatch(tenant.ContactInfo.PhoneNumber, request.PhoneNumber),
                    linkedIn: PatchHelpers.ApplyStringPatch(tenant.ContactInfo.Socials.LinkedIn, request.LinkedIn),
                    facebook: PatchHelpers.ApplyStringPatch(tenant.ContactInfo.Socials.Facebook, request.Facebook),
                    instagram: PatchHelpers.ApplyStringPatch(tenant.ContactInfo.Socials.Instagram, request.Instagram),
                    youTube: PatchHelpers.ApplyStringPatch(tenant.ContactInfo.Socials.YouTube, request.YouTube)
                );

                await tenantRepository.UpdateAsync(tenant, cancellationToken);
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}