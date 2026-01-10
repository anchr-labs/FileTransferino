namespace FileTransferino.Core.Models;

/// <summary>
/// Application settings persisted to settings.json.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// The currently active theme identifier.
    /// </summary>
    public string ActiveThemeId { get; set; } = "Default";

    /// <summary>
    /// UTC timestamp of the first application run.
    /// </summary>
    public DateTime FirstRunUtc { get; set; }

    /// <summary>
    /// UTC timestamp of the last application run.
    /// </summary>
    public DateTime LastRunUtc { get; set; }

    /// <summary>
    /// Creates default settings for first run.
    /// </summary>
    public static AppSettings CreateDefault()
    {
        var now = DateTime.UtcNow;
        return new AppSettings
        {
            ActiveThemeId = "Default",
            FirstRunUtc = now,
            LastRunUtc = now
        };
    }
}
