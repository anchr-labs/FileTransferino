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
    /// Applies a theme by its ID.
    /// </summary>
    void ApplyTheme(string themeId);
}
