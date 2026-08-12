namespace RemoteCommerce.Plugins.Abstractions;

public sealed record PluginManifest(
    string Id,
    string Name,
    string Version,
    string EntryAssembly,
    string EntryType,
    string MinHostVersion,
    string? Description = null);
