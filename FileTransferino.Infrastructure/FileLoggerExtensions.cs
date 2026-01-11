using Microsoft.Extensions.Logging;

namespace FileTransferino.Infrastructure;

/// <summary>
/// Extension methods for adding file logging.
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds a file logger to the logging builder.
    /// </summary>
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string logFilePath)
    {
        builder.AddProvider(new FileLoggerProvider(logFilePath));
        return builder;
    }
}
