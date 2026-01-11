namespace FileTransferino.Core.Models;

/// <summary>
/// Application settings persisted to settings.json.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// The currently active theme identifier.
    /// </summary>
    public string ActiveThemeId { get; set; } = "Light";

    /// <summary>
    /// The last visited theme id inside the command palette submenu. Optional.
    /// </summary>
    public string? LastVisitedThemeId { get; set; }

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
            ActiveThemeId = "Light",
            LastVisitedThemeId = null,
            FirstRunUtc = now,
            LastRunUtc = now
        };
    }
}
