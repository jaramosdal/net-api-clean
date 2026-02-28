using FluentValidation;
using FluentValidation.Results;
using Soliss.NuGetRepo.Mediator;

namespace Application.Behaviors;

internal sealed class ValidationBehavior<TInput, TOutput>(IEnumerable<IValidator<TInput>> validators) : IPipelineBehavior<TInput, TOutput>
{
    public async Task<TOutput> Handle(TInput input, Func<Task<TOutput>> next, CancellationToken cancellationToken = default)
    {
        if (validators.Any())
        {
            ValidationContext<TInput> context = new(input);
            ValidationResult[] validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            List<ValidationFailure> failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
