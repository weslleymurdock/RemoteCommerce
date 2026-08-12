namespace RemoteCommerce.Application.Pipeline;

/// <summary>Runs all FluentValidation validators registered for an application request before its handler.</summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
/// <param name="validators">The validators registered for the request type.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>Validates the request and invokes the next pipeline stage only when validation succeeds.</summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="next">The next pipeline delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response returned by the next pipeline stage.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validators reject the request.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestValidators = validators.ToArray();
        if (requestValidators.Length == 0)
        {
            return await next(cancellationToken);
        }

        var contexts = requestValidators.Select(validator => validator.ValidateAsync(request, cancellationToken));
        var results = await Task.WhenAll(contexts);
        var failures = results.SelectMany(result => result.Errors).Where(error => error is not null).ToArray();
        if (failures.Length > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
