using API.Contracts.SubTopics;
using FluentValidation;

namespace API.Validators.SubTopics
{
    public sealed class UpdateSubTopicRequestValidator : AbstractValidator<UpdateSubTopicRequest>
    {
        public UpdateSubTopicRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000);


        }
    }
}
