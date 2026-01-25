using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Users.Commands;

public class UploadUserProfilePhoto
{
    public class Command : IRequest<Result<Unit>>
    {
        public required IFormFile File { get; set; }
    }

    public class Handler(
        IPhotoService photoService,
        ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            var user = await usersRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);

            var uploadResult = await photoService.UploadPhoto(request.File);
            if (uploadResult == null)
                return Result<Unit>.Failure("Failed to upload the photo", HttpStatusCode.BadRequest);

            user.SetProfileImage(new UserImage(uploadResult.Url, uploadResult.PublicId));

            await usersRepository.UpdateAsync(user, cancellationToken);
            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
