using Microsoft.Extensions.Logging;

namespace FileTransferino.Infrastructure;

/// <summary>
/// Simple file-based logger that writes to the application logs directory.
/// Thread-safe implementation for concurrent logging.
/// </summary>
public sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public FileLogger(string categoryName, string logFilePath)
    {
        _categoryName = categoryName;
        _logFilePath = logFilePath;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";
        
        if (exception != null)
        {
            logEntry += $"\nException: {exception}";
        }

        logEntry += "\n";

        try
        {
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logEntry);
            }
        }
        catch
        {
            // Silently fail if we can't write to the log file
            // This prevents logging errors from crashing the application
        }
    }
}
