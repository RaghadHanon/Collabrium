using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Commands;

public sealed class CreateCourse
{
    public sealed class Command : IRequest<Result<CourseDto>>
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Url { get; set; }
        public required string Provider { get; set; }
        public string? InstructorName { get; set; }

        public required CourseLevel Level { get; set; }
        public required CourseLanguage Language { get; set; }
        public List<string> LearningPoints { get; set; } = new();
        public decimal? EstimatedDurationInHours { get; set; }

    }

    public sealed class Handler(
        ICoursesRepository coursesRepository,
        ICoursesReadModel coursesReadModel,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
        : IRequestHandler<Command, Result<CourseDto>>
    {
        public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<CourseDto>.Failure("Tenant not found.", System.Net.HttpStatusCode.NotFound);
            Course? course = null;
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                course = new Course(
                    tenantId: tenantId,
                    title: request.Title,
                    description: request.Description,
                    url: request.Url,
                    provider: request.Provider,
                    level: request.Level,
                    language: request.Language,
                    instructorName: request.InstructorName,
                    estimatedDurationInHours: request.EstimatedDurationInHours
                );

                course.SetLearningPoints(request.LearningPoints);

                await coursesRepository.AddAsync(course, ct);

            }, cancellationToken);

            var dto = await coursesReadModel.GetByIdAsync(tenantId, course.Id, cancellationToken);
            return dto is null
                ? Result<CourseDto>.Failure("Course created but could not be loaded.", HttpStatusCode.InternalServerError)
                : Result<CourseDto>.Success(dto, HttpStatusCode.Created);
        }
    }
}
