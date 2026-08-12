namespace RemoteCommerce.Plugins;

/// <summary>Stores a process-local restart request for presentation and future deployment orchestration.</summary>
public sealed class ApplicationRestartService : IApplicationRestartService
{
    private readonly object sync = new();
    private ApplicationRestartStatus status = new(false, null);

    /// <inheritdoc />
    public ApplicationRestartStatus Status
    {
        get
        {
            lock (sync)
                return status;
        }
    }

    /// <inheritdoc />
    public void RequestRestart(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        lock (sync)
            status = new(true, reason);
    }
}
