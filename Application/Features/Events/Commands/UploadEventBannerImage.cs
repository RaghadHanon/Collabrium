using Application.Common;
using Application.Features.Events.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Events.Commands;
public sealed class UploadEventBannerImage
{
    public sealed class Command : IRequest<Result<EventDto>>
    {
        public required string EventId { get; set; }
        public required IFormFile File { get; set; }
    }

    public sealed class Handler(
        IAttachmentStorageFactory storageFactory,
        IEventsRepository eventsRepository,
        IEventsReadModel eventsReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<EventDto>>
    {
        public async Task<Result<EventDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<EventDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();

            if (request.File is null || request.File.Length == 0)
                return Result<EventDto>.Failure("No file uploaded.", HttpStatusCode.BadRequest);

            var ev = await eventsRepository.GetByIdAsync(tenantId, request.EventId, cancellationToken);
            if (ev is null)
                return Result<EventDto>.Failure("Event not found.", HttpStatusCode.NotFound);

            var storage = storageFactory.Get(AttachmentType.Image);

            var stored = await storage.UploadAsync(
                request.File,
                AttachmentType.Image,
                folder: "Collabrium/EventBanners",
                ct: cancellationToken);

            if (stored is null)
                return Result<EventDto>.Failure("Failed to upload banner image.", HttpStatusCode.BadRequest);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                ev.SetBannerImage(stored.Url, stored.StoragePath);

                await eventsRepository.UpdateAsync(ev, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            var dto = await eventsReadModel.GetByIdAsync(tenantId, ev.Id, currentUserId, cancellationToken);

            return dto is null
                ? Result<EventDto>.Failure("Banner uploaded but event could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<EventDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}