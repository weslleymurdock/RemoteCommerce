namespace RemoteCommerce.Application.Security;

/// <summary>Reads secrets through ASP.NET Core configuration providers without persisting them.</summary>
/// <param name="configuration">The deployment configuration source.</param>
public sealed class ConfigurationSecretProvider(IConfiguration configuration) : ISecretProvider
{
    /// <inheritdoc />
    public string? Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return configuration[key];
    }

    /// <inheritdoc />
    public bool IsConfigured(string key) => !string.IsNullOrWhiteSpace(Get(key));
}
