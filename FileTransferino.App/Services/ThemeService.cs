﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using FileTransferino.Core.Models;
using FileTransferino.Infrastructure;

namespace FileTransferino.App.Services;

/// <summary>
/// Implementation of theme service that manages theme switching at runtime.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private readonly Application _app;
    private readonly SettingsStore _settingsStore;
    private readonly AppSettings _settings;
    private ResourceInclude? _currentThemeResource;
    private string _currentThemeId;

    private static readonly List<ThemeDefinition> BuiltInThemes = new()
    {
        new ThemeDefinition 
        { 
            Id = "Light", 
            DisplayName = "Light", 
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Light.axaml" 
        },
        new ThemeDefinition 
        { 
            Id = "Dark", 
            DisplayName = "Dark", 
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Dark.axaml" 
        },
        new ThemeDefinition 
        { 
            Id = "Ocean", 
            DisplayName = "Ocean", 
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Ocean.axaml" 
        },
        new ThemeDefinition 
        { 
            Id = "Nord", 
            DisplayName = "Nord", 
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Nord.axaml" 
        },
        new ThemeDefinition 
        { 
            Id = "Monokai", 
            DisplayName = "Monokai", 
            ResourcePath = "avares://FileTransferino.App/Themes/BuiltIn/Monokai.axaml" 
        }
    };

    public ThemeService(Application app, SettingsStore settingsStore, AppSettings settings)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _currentThemeId = settings.ActiveThemeId;
    }

    public IReadOnlyList<ThemeDefinition> GetThemes() => BuiltInThemes;

    public string CurrentThemeId => _currentThemeId;

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
                _app.Resources.MergedDictionaries.Remove(_currentThemeResource);
                _currentThemeResource = null;
            }

            // Load and apply new theme
            var newThemeResource = new ResourceInclude(new Uri(theme.ResourcePath));

            _app.Resources.MergedDictionaries.Add(newThemeResource);
            _currentThemeResource = newThemeResource;
            _currentThemeId = themeId;

            // Persist to settings
            _settings.ActiveThemeId = themeId;
            _settingsStore.Save(_settings);

            Debug.WriteLine($"Theme applied: {theme.DisplayName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to apply theme '{themeId}': {ex.Message}");
        }
    }
}
