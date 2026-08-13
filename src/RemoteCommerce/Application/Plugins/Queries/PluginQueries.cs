namespace RemoteCommerce.Application.Plugins.Queries;

/// <summary>Gets persisted plugin administration records.</summary>
public sealed record ListPluginsQuery : IQuery<IReadOnlyList<PluginInformation>>;

/// <summary>Gets the current application restart requirement.</summary>
public sealed record GetPluginRestartStatusQuery : IQuery<ApplicationRestartStatus>;
