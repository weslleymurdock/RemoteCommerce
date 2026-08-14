namespace RemoteCommerce.Infrastructure.Logging;

/// <summary>Registers the RemoteCommerce application file logging infrastructure.</summary>
public static class LoggingServiceCollectionExtensions
{
    /// <summary>Adds the structured application file logger and its reader.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="logging">The logging builder.</param>
    /// <param name="logDirectory">The directory in which the application log is stored.</param>
    /// <returns>The logging builder.</returns>
    public static ILoggingBuilder AddRemoteCommerceFileLogging(this ILoggingBuilder logging, IServiceCollection services, string logDirectory)
    {
        var path = Path.Combine(logDirectory, "application.log");
        logging.AddProvider(new FileLoggerProvider(path));
        services.AddSingleton(new ApplicationLogReader(path));
        return logging;
    }
}
