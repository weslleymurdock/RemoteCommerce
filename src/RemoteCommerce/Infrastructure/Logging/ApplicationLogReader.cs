namespace RemoteCommerce.Infrastructure.Logging;

/// <summary>Reads recent application log records for the administrative viewer.</summary>
public sealed class ApplicationLogReader
{
    private readonly string _filePath;

    /// <summary>Initializes the application log reader.</summary>
    /// <param name="filePath">The absolute application log path.</param>
    public ApplicationLogReader(string filePath) => _filePath = filePath;

    /// <summary>Reads the most recent application log lines.</summary>
    /// <param name="maxLines">The maximum number of lines to return.</param>
    /// <returns>Recent formatted log records.</returns>
    public IReadOnlyList<string> ReadRecent(int maxLines = 500)
    {
        if (!File.Exists(_filePath)) return [];
        maxLines = Math.Clamp(maxLines, 1, 5000);
        return File.ReadLines(_filePath).TakeLast(maxLines).ToArray();
    }
}
