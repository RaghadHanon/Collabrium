using API.Contracts.WorkItems.Relations;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.WorkItems.Relations;

public class GetWorkItemRelationsRequestValidator : AbstractValidator<GetWorkItemRelationsRequest>
{
    public GetWorkItemRelationsRequestValidator()
    {
        RuleFor(x => x.RelationType)
            .IsInEnum()
            .WithMessage("RelationType must be a valid RelationType value.")
            .When(x => !(x.RelationType is null));
    }
}