namespace RemoteCommerce.Application.Localization.Abstractions;

/// <summary>Provides resource-type-aware localized strings to application and UI code.</summary>
public interface ILocalizer
{
    /// <summary>Gets a localized value for the specified resource type and key.</summary>
    /// <typeparam name="TResource">The marker type identifying the resource namespace.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <param name="arguments">Optional formatting arguments.</param>
    /// <returns>The localized string, or the key when no translation exists.</returns>
    string Get<TResource>(string key, params object[] arguments);

    /// <summary>Gets a localized value for an explicit resource type and key.</summary>
    /// <param name="resourceType">The resource marker type.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="arguments">Optional formatting arguments.</param>
    /// <returns>The localized string, or the key when no translation exists.</returns>
    string Get(Type resourceType, string key, params object[] arguments);
}
