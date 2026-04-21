using API.Contracts.UserCourses;
using FluentValidation;

namespace API.Validators.UserCourses
{
    public sealed class UnassignCourseFromUserRequestValidator : AbstractValidator<UnassignCourseFromUserRequest>
    {
        public UnassignCourseFromUserRequestValidator()
        {
            RuleFor(x => x.AssignedToUserId)
                .NotEmpty()
                .MaximumLength(450);
        }
    }
}
