using FluentValidation;
using MediatR.Pipeline;

namespace LinkShorter.Application.Features;

public class ValidationPreProcessor<TRequest> : IRequestPreProcessor<TRequest> 
    where TRequest : notnull
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationPreProcessor(
        IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken) =>
        _validator?.ValidateAsync(
            request,
            options => options.ThrowOnFailures(),
            cancellationToken) ??  Task.CompletedTask;
}