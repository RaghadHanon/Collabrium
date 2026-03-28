using Application.Common;
using Application.Features.CourseAssignments.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.CourseAssignments.Commands
{
    public sealed class CompleteCourseAssignment
    {
        public sealed class Command : IRequest<Result<CourseAssignmentDto>>
        {
            public required string AssignmentId { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsRepository courseAssignmentsRepository,
            ICourseAssignmentsReadModel courseAssignmentsReadModel,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<CourseAssignmentDto>>
        {
            public async Task<Result<CourseAssignmentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseAssignmentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var assignment = await courseAssignmentsRepository.GetByIdAsync(
                    tenantId,
                    request.AssignmentId,
                    cancellationToken);

                if (assignment is null)
                    return Result<CourseAssignmentDto>.Failure("Course assignment not found.", HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    assignment.MarkAsCompleted();
                    await courseAssignmentsRepository.UpdateAsync(assignment, ct);
                }, cancellationToken);

                var dto = await courseAssignmentsReadModel.GetByIdAsync(
                    tenantId,
                    assignment.Id,
                    cancellationToken);

                return dto is null
                    ? Result<CourseAssignmentDto>.Failure("Course assignment updated but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<CourseAssignmentDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
