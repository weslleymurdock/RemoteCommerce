namespace RemoteCommerce.Application.Common.Behaviors;

/// <summary>Runs FluentValidation validators before an application request reaches its handler.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var registered = validators.ToArray();
        if (registered.Length == 0) return await next(cancellationToken);
        var results = await Task.WhenAll(registered.Select(x => x.ValidateAsync(request, cancellationToken)));
        var failures = results.SelectMany(x => x.Errors).Where(x => x is not null).ToArray();
        if (failures.Length != 0) throw new ValidationException(failures);
        return await next(cancellationToken);
    }
}
