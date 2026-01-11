using System.Text.Json;
using FileTransferino.Core.Models;

namespace FileTransferino.Infrastructure;

/// <summary>
/// Handles loading and saving application settings to/from JSON file.
/// </summary>
public sealed class SettingsStore
{
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SettingsStore(AppPaths paths)
    {
        _settingsPath = Path.Combine(paths.Root, "settings.json");
    }

    /// <summary>
    /// Loads settings from disk. Creates defaults if file doesn't exist.
    /// </summary>
    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = AppSettings.CreateDefault();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? AppSettings.CreateDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            return AppSettings.CreateDefault();
        }
    }

    /// <summary>
    /// Loads settings from disk asynchronously. Creates defaults if file doesn't exist.
    /// </summary>
    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = AppSettings.CreateDefault();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        try
        {
            await using var stream = File.OpenRead(_settingsPath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken);
            return settings ?? AppSettings.CreateDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            return AppSettings.CreateDefault();
        }
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    /// <summary>
    /// Saves settings to disk asynchronously.
    /// </summary>
    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }
}
