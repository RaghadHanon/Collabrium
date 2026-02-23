using API.Contracts.Users;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Users;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.NewRole)
            .Must(v => Enum.TryParse<UserRole>(v, true, out _))
            .WithMessage("User role is not valid.");
    }
}
