using Application.Common;
using Application.Interfaces;
using Domain.Entities;
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

    public class Handler(IPhotoService photoService, ICurrentUserAccessor currentUserAccessor,
        IUsersRepository usersRepository) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentUser = await currentUserAccessor.GetUserAsync();
            if (currentUser == null)
            {
                return Result<Unit>.Failure("User not found.", HttpStatusCode.NotFound);
            }

            var uploadResult = await photoService.UploadPhoto(request.File);
            if (uploadResult == null)
                return Result<Unit>.Failure("Failed to upload the photo", HttpStatusCode.BadRequest);

            var image = new UserImage
            {
                Url = uploadResult.Url,
                PublicId = uploadResult.PublicId,
                ApplicationUserId = currentUser.Id
            };
            currentUser.ProfileImage = image;
            await usersRepository.SaveChangesAsync();

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
        }
    }
}
