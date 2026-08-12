namespace RemoteCommerce.Plugins;

/// <summary>Provides access to packages from an explicitly trusted local package source.</summary>
public interface IPluginPackageSource
{
    /// <summary>Gets candidate package paths available from the trusted source.</summary>
    /// <param name="cancellationToken">The token used to cancel package enumeration.</param>
    /// <returns>The trusted <c>.nupkg</c> package paths.</returns>
    Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default);
}

/// <summary>Configures a local directory whose packages are explicitly trusted for administrative installation.</summary>
public sealed class TrustedPluginPackageSource : IPluginPackageSource
{
    private readonly string? directory;

    /// <summary>Initializes a trusted local package source.</summary>
    /// <param name="configuration">The host configuration containing <c>PluginAdministration:TrustedPackageDirectory</c>.</param>
    public TrustedPluginPackageSource(IConfiguration configuration)
    {
        directory = configuration["PluginAdministration:TrustedPackageDirectory"];
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return Task.FromResult<IReadOnlyList<string>>([]);

        var files = Directory.EnumerateFiles(directory, "*.nupkg", SearchOption.TopDirectoryOnly)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return Task.FromResult<IReadOnlyList<string>>(files);
    }
}
