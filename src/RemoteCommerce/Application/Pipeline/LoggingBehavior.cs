namespace RemoteCommerce.Application.Pipeline;

/// <summary>Logs MediatR request execution without serializing request payloads or secrets.</summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
/// <param name="logger">The logger used to record request execution.</param>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>Executes the next pipeline stage while recording request type, duration, and failure status.</summary>
    /// <param name="request">The current request.</param>
    /// <param name="next">The next pipeline delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response returned by the next pipeline stage.</returns>
    /// <exception cref="Exception">Propagates any exception raised by the next pipeline stage.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Handling application request {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Completed application request {RequestName} in {ElapsedMilliseconds} ms", requestName, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Application request {RequestName} failed after {ElapsedMilliseconds} ms", requestName, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
    }
}
