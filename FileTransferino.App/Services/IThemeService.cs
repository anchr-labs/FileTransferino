namespace FileTransferino.App.Services;

/// <summary>
/// Represents a theme definition.
/// </summary>
public sealed class ThemeDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string ResourcePath { get; init; }
}

/// <summary>
/// Service for managing application themes.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets all available themes.
    /// </summary>
    IReadOnlyList<ThemeDefinition> GetThemes();
    
    /// <summary>
    /// Gets the currently active theme ID.
    /// </summary>
    string CurrentThemeId { get; }
    
    /// <summary>
    /// Gets or sets the last visited theme id (in command palette submenu). Optional.
    /// Persisted via settings.
    /// </summary>
    string? LastVisitedThemeId { get; set; }

    /// <summary>
    /// Applies a theme by its ID.
    /// </summary>
    void ApplyTheme(string themeId);

    /// <summary>
    /// Preview a theme by its ID without persisting the change.
    /// </summary>
    void PreviewTheme(string themeId);

    /// <summary>
    /// Re-apply the persisted active theme without changing persisted settings.
    /// </summary>
    void RestoreActiveTheme();
}
