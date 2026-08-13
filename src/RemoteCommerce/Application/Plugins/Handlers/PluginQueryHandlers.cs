namespace RemoteCommerce.Application.Plugins.Handlers;

/// <summary>Handles plugin administration queries.</summary>
public sealed class ListPluginsQueryHandler(PluginManagementService managementService) : IRequestHandler<ListPluginsQuery, IReadOnlyList<PluginInformation>>
{
    /// <inheritdoc />
    public Task<IReadOnlyList<PluginInformation>> Handle(ListPluginsQuery request, CancellationToken cancellationToken) => managementService.ListAsync(cancellationToken);
}

/// <summary>Handles plugin restart-status queries.</summary>
public sealed class GetPluginRestartStatusQueryHandler(IApplicationRestartService restartService) : IRequestHandler<GetPluginRestartStatusQuery, ApplicationRestartStatus>
{
    /// <inheritdoc />
    public Task<ApplicationRestartStatus> Handle(GetPluginRestartStatusQuery request, CancellationToken cancellationToken) => Task.FromResult(restartService.Status);
}
