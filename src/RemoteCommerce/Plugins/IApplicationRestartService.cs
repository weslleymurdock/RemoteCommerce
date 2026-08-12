namespace RemoteCommerce.Plugins;

/// <summary>Coordinates the host's restart-required lifecycle without choosing a process supervisor strategy.</summary>
public interface IApplicationRestartService
{
    /// <summary>Marks the application as requiring a restart.</summary>
    /// <param name="reason">The administrative reason for the restart request.</param>
    void RequestRestart(string reason);

    /// <summary>Gets the current restart requirement, when one has been requested.</summary>
    ApplicationRestartStatus Status { get; }
}

/// <summary>Describes the current application restart requirement.</summary>
/// <param name="Required">Indicates whether a restart is required.</param>
/// <param name="Reason">The reason associated with the restart request.</param>
public sealed record ApplicationRestartStatus(bool Required, string? Reason);
