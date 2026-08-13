namespace RemoteCommerce.Application.Identity;

/// <summary>Provides an application boundary for sending identity-related email messages.</summary>
/// <typeparam name="TMessage">The marker type identifying the email template family.</typeparam>
public interface IEmailService<TMessage>
{
    /// <summary>Sends an identity email.</summary><param name="recipient">The recipient address.</param><param name="subject">The email subject.</param><param name="body">The non-secret email body.</param><param name="cancellationToken">The cancellation token.</param>
    Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken);
}

/// <summary>Identifies RemoteCommerce Identity email messages.</summary>
public sealed class IdentityEmailMessage { }

/// <summary>Provides the initial development email implementation by logging delivery metadata without secrets.</summary>
public sealed class LoggingIdentityEmailService(ILogger<LoggingIdentityEmailService> logger) : IEmailService<IdentityEmailMessage>
{
    /// <inheritdoc />
    public Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken) { logger.LogInformation("Identity email queued for {Recipient} with subject {Subject}. Body length: {Length}.", recipient, subject, body.Length); return Task.CompletedTask; }
}
