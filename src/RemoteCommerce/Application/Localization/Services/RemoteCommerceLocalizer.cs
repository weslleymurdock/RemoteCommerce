namespace RemoteCommerce.Application.Localization.Services;

/// <summary>Resolves imported RemoteCommerce resources with culture fallback and ASP.NET Core fallback.</summary>
/// <param name="factory">The ASP.NET Core localizer factory used for embedded application resources.</param>
/// <param name="resourceService">The imported resource service.</param>
public sealed class RemoteCommerceLocalizer(IStringLocalizerFactory factory, ILocalizationResourceService resourceService) : ILocalizer
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> cache = new();

    /// <inheritdoc />
    public string Get<TResource>(string key, params object[] arguments) => GetAsync<TResource>(key, arguments).GetAwaiter().GetResult();

    /// <inheritdoc />
    public string Get(Type resourceType, string key, params object[] arguments) => GetAsync(resourceType, key, arguments).GetAwaiter().GetResult();

    /// <summary>Resolves a localized resource asynchronously using the current UI culture and en-US fallback.</summary>
    /// <typeparam name="TResource">The resource marker type.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <param name="arguments">Optional formatting arguments.</param>
    /// <returns>The resolved localized value or the key when no value exists.</returns>
    public Task<string> GetAsync<TResource>(string key, params object[] arguments) => GetAsync(typeof(TResource), key, arguments);

    /// <summary>Resolves a localized resource asynchronously for an explicit resource type.</summary>
    /// <param name="resourceType">The resource marker type.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="arguments">Optional formatting arguments.</param>
    /// <returns>The resolved localized value or the key when no value exists.</returns>
    public async Task<string> GetAsync(Type resourceType, string key, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var resourceName = resourceType.FullName ?? resourceType.Name;
        var culture = CultureInfo.CurrentUICulture;

        foreach (var candidate in GetCultureFallbacks(culture))
        {
            var entries = await GetEntriesAsync(resourceName, candidate);
            if (entries is not null && entries.TryGetValue(key, out var value))
            {
                return Format(value, arguments);
            }
        }

        var embedded = factory.Create(resourceType);
        return embedded[key, arguments].Value ?? key;
    }

    /// <summary>Clears the cached imported resource for a culture and resource type.</summary>
    /// <param name="resourceType">The resource marker type.</param>
    /// <param name="culture">The culture whose cache entry must be invalidated.</param>
    public void Invalidate(Type resourceType, string culture)
    {
        var resourceName = resourceType.FullName ?? resourceType.Name;
        cache.TryRemove(BuildCacheKey(resourceName, culture), out _);
    }

    private async Task<IReadOnlyDictionary<string, string>?> GetEntriesAsync(string resourceType, string culture)
    {
        var key = BuildCacheKey(resourceType, culture);
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var entries = await resourceService.GetActiveEntriesAsync(resourceType, culture);
        if (entries is null)
        {
            return null;
        }

        cache[key] = entries;
        return entries;
    }

    private static IEnumerable<string> GetCultureFallbacks(CultureInfo culture)
    {
        yield return culture.Name;
        if (!string.IsNullOrEmpty(culture.Parent.Name))
        {
            yield return culture.Parent.Name;
        }

        if (!string.Equals(culture.Name, "en-US", StringComparison.OrdinalIgnoreCase))
        {
            yield return "en-US";
        }
    }

    private static string BuildCacheKey(string resourceType, string culture) => $"{resourceType}|{culture}";

    private static string Format(string value, object[] arguments)
        => arguments.Length == 0
            ? value
            : string.Format(CultureInfo.CurrentCulture, value, arguments);
}
