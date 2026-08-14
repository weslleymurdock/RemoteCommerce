namespace RemoteCommerce.Infrastructure.Logging;

/// <summary>Writes application log records to a single structured text file.</summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.Ordinal);

    /// <summary>Initializes the application file logger provider.</summary>
    /// <param name="filePath">The absolute path of the application log file.</param>
    public FileLoggerProvider(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory);
    }

    /// <summary>Creates or reuses a logger for the specified category.</summary>
    /// <param name="categoryName">The logger category, normally the fully qualified class name.</param>
    /// <returns>The file logger.</returns>
    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, name => new FileLogger(name, Write));

    /// <summary>Releases logger resources.</summary>
    public void Dispose() => _loggers.Clear();

    private void Write(string category, LogLevel level, EventId eventId, string message, Exception? exception)
    {
        var line = $"[{DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}][{level}][{category}][{message}]";
        if (exception is not null)
        {
            line += $" ExceptionType={exception.GetType().FullName} ExceptionMessage={exception.Message}";
        }

        lock (_sync)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    private sealed class FileLogger(string category, Action<string, LogLevel, EventId, string, Exception?> writer) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            writer(category, logLevel, eventId, formatter(state, exception), exception);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
