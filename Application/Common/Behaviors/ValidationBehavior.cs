using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validator == null) return await next();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        return !validationResult.IsValid ? throw new ValidationException(validationResult.Errors) : await next();
    }
}