using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Commands;

public sealed class DeleteCourse
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public required string CourseId { get; set; }
    }

    public sealed class Handler(
        ICoursesRepository coursesRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var course = await coursesRepository.GetByIdAsync(
                tenantId,
                request.CourseId,
                cancellationToken);

            if (course is null)
                return Result<Unit>.Failure("Course not found.", HttpStatusCode.NotFound);

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await coursesRepository.DeleteAsync(
                    tenantId,
                    request.CourseId,
                    ct);

            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value, HttpStatusCode.OK);
        }
    }
}
