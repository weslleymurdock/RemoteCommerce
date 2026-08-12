namespace RemoteCommerce.Application.Pipeline;

/// <summary>Converts FluentValidation failures into HTTP 400 validation problem details.</summary>
public sealed class ValidationExceptionHandler : IExceptionHandler
{
    /// <summary>Handles a validation exception when one is raised at the HTTP boundary.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The exception raised by the application pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the exception was a FluentValidation failure; otherwise <see langword="false"/>.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var errors = validationException.Errors
            .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors), cancellationToken);
        return true;
    }
}
