namespace RemoteCommerce.Application.Identity.Services;

/// <summary>Provides the initial development email implementation by logging delivery metadata without secrets.</summary>
public sealed class LoggingIdentityEmailService(ILogger<LoggingIdentityEmailService> logger) : IEmailService<IdentityEmailMessage>
{
    /// <inheritdoc />
    public Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Identity email queued for {Recipient} with subject {Subject}. Body length: {Length}.",
            recipient,
            subject,
            body.Length);

        return Task.CompletedTask;
    }
}
