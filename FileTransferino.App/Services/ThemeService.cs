using System.Diagnostics;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using FileTransferino.Core.Models;
using FileTransferino.Infrastructure;

namespace FileTransferino.App.Services;

/// <summary>
/// Implementation of theme service that manages theme switching at runtime.
/// </summary>
public sealed class ThemeService(
    Application app,
    SettingsStore settingsStore,
    AppSettings settings,
    ResourceInclude? currentThemeResource) : IThemeService
{
    private ResourceInclude? _currentThemeResource = currentThemeResource;

    private static readonly List<ThemeDefinition> BuiltInThemes =
    [
        new()
        {
            Id = "Light",
            DisplayName = "Light",
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Light.axaml"
        },

        new()
        {
            Id = "Dark",
            DisplayName = "Dark",
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Dark.axaml"
        },

        new()
        {
            Id = "Ocean",
            DisplayName = "Ocean",
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Ocean.axaml"
        },

        new()
        {
            Id = "Nord",
            DisplayName = "Nord",
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Nord.axaml"
        },

        new()
        {
            Id = "Monokai",
            DisplayName = "Monokai",
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Monokai.axaml"
        }
    ];

    public IReadOnlyList<ThemeDefinition> GetThemes() => BuiltInThemes;

    public string CurrentThemeId { get; private set; } = settings.ActiveThemeId;

    public void ApplyTheme(string themeId)
    {
        var theme = BuiltInThemes.FirstOrDefault(t => t.Id == themeId);
        if (theme == null)
        {
            Debug.WriteLine($"Theme '{themeId}' not found. Using Light theme.");
            theme = BuiltInThemes.First(t => t.Id == "Light");
            themeId = "Light";
        }

        try
        {
            // Remove existing theme resource if present
            if (_currentThemeResource != null)
            {
                app.Resources.MergedDictionaries.Remove(_currentThemeResource);
                _currentThemeResource = null;
            }

            // Load and apply new theme - must set Source property
            var newThemeResource = new ResourceInclude(new Uri("avares://FileTransferino.App/"))
            {
                Source = new Uri(theme.ResourcePath)
            };

            app.Resources.MergedDictionaries.Add(newThemeResource);
            _currentThemeResource = newThemeResource;
            CurrentThemeId = themeId;

            // Persist to settings
            settings.ActiveThemeId = themeId;
            settingsStore.Save(settings);

            Debug.WriteLine($"Theme applied: {theme.DisplayName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to apply theme '{themeId}': {ex.Message}");
        }
    }
}
