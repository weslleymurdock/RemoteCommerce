namespace RemoteCommerce.Application.Security.Abstractions;

/// <summary>Provides application access to deployment-managed secret values.</summary>
public interface ISecretProvider
{
    /// <summary>Gets a configured secret without persisting or exposing it through administration APIs.</summary>
    /// <param name="key">The configuration key identifying the secret.</param>
    /// <returns>The secret value, or <see langword="null"/> when it is not configured.</returns>
    string? Get(string key);

    /// <summary>Determines whether a secret is configured.</summary>
    /// <param name="key">The configuration key identifying the secret.</param>
    /// <returns><see langword="true"/> when a non-empty secret is configured; otherwise, <see langword="false"/>.</returns>
    bool IsConfigured(string key);
}
