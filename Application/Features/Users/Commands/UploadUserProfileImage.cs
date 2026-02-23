using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Users.Commands;

public class UploadUserProfileImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required IFormFile File { get; set; }
    }

    public class Handler(
        IAttachmentStorageFactory storageFactory,
        ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            var user = await usersRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            if (request.File is null || request.File.Length == 0)
                return Result<Unit>.Failure("No file uploaded.", HttpStatusCode.BadRequest);

            var storage = storageFactory.Get(AttachmentType.Image);

            var stored = await storage.UploadAsync(
                request.File,
                AttachmentType.Image,
                folder: "Collabrium/ProfileImages",
                ct: cancellationToken);

            if (stored is null)
                return Result<Unit>.Failure("Failed to upload the photo.", HttpStatusCode.BadRequest);

            user.SetProfileImage(stored.Url, stored.StoragePath);

            await usersRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}