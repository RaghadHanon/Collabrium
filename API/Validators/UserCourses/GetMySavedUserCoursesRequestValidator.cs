using API.Contracts.UserCourses;
using FluentValidation;

namespace API.Validators.UserCourses;

public sealed class GetMySavedUserCoursesRequestValidator : AbstractValidator<GetMySavedUserCoursesRequest>
{
    public GetMySavedUserCoursesRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x => !x.AssignedAfter.HasValue || !x.AssignedBefore.HasValue || x.AssignedAfter <= x.AssignedBefore)
            .WithMessage("AssignedAfter must be less than or equal to AssignedBefore.");

        RuleFor(x => x)
            .Must(x => !x.CreatedAfter.HasValue || !x.CreatedBefore.HasValue || x.CreatedAfter <= x.CreatedBefore)
            .WithMessage("CreatedAfter must be less than or equal to CreatedBefore.");

        RuleFor(x => x)
            .Must(x => !x.UpdatedAfter.HasValue || !x.UpdatedBefore.HasValue || x.UpdatedAfter <= x.UpdatedBefore)
            .WithMessage("UpdatedAfter must be less than or equal to UpdatedBefore.");
    }
}
