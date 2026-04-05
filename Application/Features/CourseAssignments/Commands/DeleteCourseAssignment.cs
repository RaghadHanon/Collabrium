using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.CourseAssignments.Commands
{
    public sealed class DeleteCourseAssignment
    {
        public sealed class Command : IRequest<Result<Unit>>
        {
            public required string AssignmentId { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsRepository courseAssignmentsRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<Unit>>
        {
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<Unit>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var assignment = await courseAssignmentsRepository.GetByIdAsync(
                    tenantId,
                    request.AssignmentId,
                    cancellationToken);

                if (assignment is null)
                    return Result<Unit>.Failure("Course assignment not found.", HttpStatusCode.NotFound);

                var removed = false;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    removed = await courseAssignmentsRepository.RemoveAsync(
                        tenantId,
                        request.AssignmentId,
                        ct);

                    if (removed)
                        await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                return !removed
                    ? Result<Unit>.Failure("Course assignment not found.", HttpStatusCode.NotFound)
                    : Result<Unit>.Success(Unit.Value, HttpStatusCode.OK);
            }
        }
    }
}
