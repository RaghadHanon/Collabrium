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

            try
            {
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
                        description: request.Description ?? tenant.Description,
                        country: request.Country ?? tenant.Country,
                        websiteUrl: request.WebsiteUrl ?? tenant.WebsiteUrl
                    );

                    tenant.SetContactInfo(
                        email: request.Email ?? tenant.ContactInfo.Email,
                        phone: request.PhoneNumber ?? tenant.ContactInfo.PhoneNumber,
                        linkedIn: request.LinkedIn ?? tenant.ContactInfo.Socials.LinkedIn,
                        facebook: request.Facebook ?? tenant.ContactInfo.Socials.Facebook,
                        instagram: request.Instagram ?? tenant.ContactInfo.Socials.Instagram,
                        youTube: request.YouTube ?? tenant.ContactInfo.Socials.YouTube
                    );

                    await tenantRepository.UpdateAsync(tenant, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Photo upload failed"))
            {
                return Result<Unit>.Failure("Photo upload failed.", HttpStatusCode.BadRequest);
            }
        }
    }
}