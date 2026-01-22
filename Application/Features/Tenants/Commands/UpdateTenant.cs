using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
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

        // Contact Info
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? YouTube { get; set; }
    }

    public class Handler(
        IMapper mapper,
        IPhotoService photoService,
        ITenantRepository tenantRepository,
        ITenantContext tenantContext)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenant = tenantContext.CurrentTenant;

            if (tenant == null)
            {
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);
            }

            mapper.Map(request, tenant);
            tenant.TenantContactInfo ??= new TenantContactInfo
            {
                TenantId = tenant.Id
            };

            mapper.Map(request, tenant.TenantContactInfo);

            if (request.File != null)
            {
                var uploadResult = await photoService.UploadPhoto(request.File);
                if (uploadResult == null)
                {
                    return Result<Unit>.Failure("Photo upload failed.", HttpStatusCode.BadRequest);
                }

                var image = new TenantImage
                {
                    Url = uploadResult.Url,
                    PublicId = uploadResult.PublicId,
                    TenantId = tenant.Id
                };

                tenant.ProfileImage = image;
            }

            await tenantRepository.SaveChangesAsync();

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
