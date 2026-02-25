using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps
{
    public sealed class GetManagerRoadmapsRequestValidator
     : AbstractValidator<GetManagerRoadmapsRequest>
    {
        public GetManagerRoadmapsRequestValidator()
        {
            Include(new GetPublicRoadmapsRequestValidator());
        }
    }
}
