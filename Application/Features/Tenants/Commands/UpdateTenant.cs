using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Tenants.Commands;

public sealed class UpdateTenant
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public string? Description { get; init; }
        public string? Country { get; init; }
        public string? WebsiteUrl { get; init; }

        public IFormFile? File { get; init; }

        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }

        public string? LinkedIn { get; init; }
        public string? Facebook { get; init; }
        public string? Instagram { get; init; }
        public string? YouTube { get; init; }
    }

    public sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IPhotoService _photoService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _uow;

        public Handler(
            IPhotoService photoService,
            ITenantRepository tenantRepository,
            ITenantContext tenantContext,
            IUnitOfWork uow)
        {
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = _tenantContext.CurrentTenant;
            if (tenant is null)
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            // 1) upload image (optional)
            string? uploadedUrl = null;
            string? uploadedPublicId = null;

            if (request.File is not null)
            {
                var upload = await _photoService.UploadPhoto(request.File);
                if (upload is null)
                    return Result<Unit>.Failure("Photo upload failed.", HttpStatusCode.BadRequest);

                uploadedUrl = upload.Url;
                uploadedPublicId = upload.PublicId;
            }

            // 2) update profile (null means "no change" is NOT supported by your Tenant.UpdateProfile)
            // Tenant.UpdateProfile will overwrite with null -> so we must MERGE manually.
            tenant.UpdateProfile(
                description: request.Description ?? tenant.Description,
                country: request.Country ?? tenant.Country,
                websiteUrl: request.WebsiteUrl ?? tenant.WebsiteUrl
            );

            // 3) merge contact info (because SetContactInfo replaces)
            var current = tenant.ContactInfo;

            var mergedEmail = request.Email ?? current.Email;
            var mergedPhone = request.PhoneNumber ?? current.PhoneNumber;

            var mergedLinkedIn = request.LinkedIn ?? current.Socials.LinkedIn;
            var mergedFacebook = request.Facebook ?? current.Socials.Facebook;
            var mergedInstagram = request.Instagram ?? current.Socials.Instagram;
            var mergedYouTube = request.YouTube ?? current.Socials.YouTube;

            tenant.SetContactInfo(
                email: mergedEmail,
                phone: mergedPhone,
                linkedIn: mergedLinkedIn,
                facebook: mergedFacebook,
                instagram: mergedInstagram,
                youTube: mergedYouTube
            );

            // 4) set profile image if uploaded
            if (!string.IsNullOrWhiteSpace(uploadedUrl) && !string.IsNullOrWhiteSpace(uploadedPublicId))
            {
                tenant.SetProfileImage(new Domain.ValueObjects.StoredImage(uploadedUrl, uploadedPublicId));
            }

            // 5) persist
            await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}