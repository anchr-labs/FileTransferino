using System.Reflection;
using DbUp;
using FileTransferino.Infrastructure;

namespace FileTransferino.Data;

/// <summary>
/// Handles SQLite database creation and migration using DbUp.
/// </summary>
public sealed class DatabaseBootstrapper
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseBootstrapper(AppPaths paths)
    {
        _dbPath = Path.Combine(paths.Data, "FileTransferino.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    /// <summary>
    /// Gets the database file path.
    /// </summary>
    public string DatabasePath => _dbPath;

    /// <summary>
    /// Gets the connection string for the database.
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Bootstraps the database: creates file if needed and runs migrations.
    /// </summary>
    public DatabaseBootstrapResult Bootstrap()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Database path: {_dbPath}");
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Run DbUp migrations using SQLite helpers
            var upgrader = DeployChanges.To
                .SqliteDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransaction()
                .LogToTrace()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                System.Diagnostics.Debug.WriteLine($"Database migration failed: {result.Error?.Message}");
                return new DatabaseBootstrapResult(false, result.Error?.Message ?? "Unknown error");
            }

            System.Diagnostics.Debug.WriteLine("Database bootstrap completed successfully.");
            return new DatabaseBootstrapResult(true, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database bootstrap exception: {ex.Message}");
            return new DatabaseBootstrapResult(false, ex.Message);
        }
    }

    /// <summary>
    /// Bootstraps the database asynchronously.
    /// </summary>
    public Task<DatabaseBootstrapResult> BootstrapAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(Bootstrap, cancellationToken);
    }
}

/// <summary>
/// Result of database bootstrap operation.
/// </summary>
public sealed record DatabaseBootstrapResult(bool Success, string? ErrorMessage);
