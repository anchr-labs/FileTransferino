using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
            // Ensure the directory exists before writing
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logEntry);
            }
        }
        catch (Exception ex)
        {
            // Fallback to Debug output if file logging fails
            // This ensures critical errors are still captured
            Debug.WriteLine($"[FileLogger] Failed to write to log file: {ex.Message}");
            Debug.WriteLine($"[FileLogger] Original log entry: {logEntry}");
        }
    }
}
