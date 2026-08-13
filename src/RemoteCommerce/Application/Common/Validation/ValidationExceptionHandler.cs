namespace RemoteCommerce.Application.Common.Validation;

/// <summary>Converts FluentValidation failures into HTTP 400 validation problem details.</summary>
public sealed class ValidationExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException) return false;
        var errors = validationException.Errors.GroupBy(x => x.PropertyName, StringComparer.Ordinal).ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors), cancellationToken);
        return true;
    }
}
