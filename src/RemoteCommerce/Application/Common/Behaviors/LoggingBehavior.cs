namespace RemoteCommerce.Application.Common.Behaviors;

/// <summary>Logs MediatR request execution without serializing request payloads.</summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();
        logger.LogInformation(
            "Handling application request {RequestName}",
            requestName);

        try
        {
            return await next(cancellationToken);
        }
        catch (OperationCanceledException exception)
        {
            logger.LogWarning(
                exception,
                "Application request {RequestName} was cancelled after {ElapsedMilliseconds} ms",
                requestName,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Application request {RequestName} failed after {ElapsedMilliseconds} ms",
                requestName,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
        finally
        {
            logger.LogDebug(
                "Application request {RequestName} completed after {ElapsedMilliseconds} ms",
                requestName,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }
}
