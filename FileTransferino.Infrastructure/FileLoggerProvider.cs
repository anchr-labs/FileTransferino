using Microsoft.Extensions.Logging;

namespace FileTransferino.Infrastructure;

/// <summary>
/// Provider for file-based loggers.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;

    public FileLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logFilePath);
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
