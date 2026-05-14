using API.Contracts.UserCourses;
using FluentValidation;

namespace API.Validators.UserCourses;

public sealed class AssignCourseToUserRequestValidator : AbstractValidator<AssignCourseToUserRequest>
{
    public AssignCourseToUserRequestValidator()
    {
        RuleFor(x => x.AssignedToUserId)
            .NotEmpty()
            .MaximumLength(450);
    }
}
