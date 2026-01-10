namespace FileTransferino.Infrastructure;

/// <summary>
/// Provides cross-platform application directory paths.
/// Ensures all directories exist upon initialization.
/// </summary>
public sealed class AppPaths
{
    private const string AppName = "FileTransferino";

    /// <summary>
    /// Root application data directory.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Data directory for database and other data files.
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// Themes directory for UI theme files.
    /// </summary>
    public string Themes { get; }

    /// <summary>
    /// Logs directory for application log files.
    /// </summary>
    public string Logs { get; }

    public AppPaths()
    {
        // Use ApplicationData which is cross-platform via .NET APIs:
        // Windows: C:\Users\{user}\AppData\Roaming
        // macOS: /Users/{user}/.config
        // Linux: /home/{user}/.config
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        Root = Path.Combine(appData, AppName);
        Data = Path.Combine(Root, "data");
        Themes = Path.Combine(Root, "themes");
        Logs = Path.Combine(Root, "logs");

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(Themes);
        Directory.CreateDirectory(Logs);
    }
}
