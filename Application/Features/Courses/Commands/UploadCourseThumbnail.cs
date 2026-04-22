using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Features.Courses.Commands
{
    public class UploadCourseThumbnail
    {
        public class Command : IRequest<Result<Unit>>
        {
            public required string CourseId { get; set; }
            public required IFormFile File { get; set; }
        }

        public class Handler(
            IAttachmentStorageFactory storageFactory,
            ITenantContext tenantContext,
            ICoursesRepository coursesRepository,
            IUnitOfWork unitOfWork
        ) : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id?.Trim();
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<Unit>.Failure("Current tenant not found.", HttpStatusCode.NotFound);

                if (string.IsNullOrWhiteSpace(request.CourseId))
                    return Result<Unit>.Failure("Course id is required.", HttpStatusCode.BadRequest);

                if (request.File is null || request.File.Length == 0)
                    return Result<Unit>.Failure("No file uploaded.", HttpStatusCode.BadRequest);

                var course = await coursesRepository.GetByIdAsync(tenantId, request.CourseId, cancellationToken);
                if (course is null)
                    return Result<Unit>.Failure("Course not found.", HttpStatusCode.NotFound);

                var storage = storageFactory.Get(AttachmentType.Image);

                var stored = await storage.UploadAsync(
                    request.File,
                    AttachmentType.Image,
                    folder: "Collabrium/CourseThumbnails",
                    ct: cancellationToken);

                if (stored is null)
                    return Result<Unit>.Failure("Failed to upload the thumbnail.", HttpStatusCode.BadRequest);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    course.SetThumbnailImage(stored.Url, stored.StoragePath);

                    await coursesRepository.UpdateAsync(course, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                return Result<Unit>.Success(Unit.Value, HttpStatusCode.NoContent);
            }
        }
    }
}
