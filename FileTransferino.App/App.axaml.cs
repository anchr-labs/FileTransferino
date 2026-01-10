using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTransferino.App.Services;
using FileTransferino.Core.Models;
using FileTransferino.Data;
using FileTransferino.Infrastructure;

namespace FileTransferino.App;

public class App : Application
{
    private AppPaths? _appPaths;
    private SettingsStore? _settingsStore;
    private AppSettings? _settings;
    private DatabaseBootstrapper? _dbBootstrapper;
    private ThemeService? _themeService;

    public IThemeService? ThemeService => _themeService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Show window immediately - don't block on initialization
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;

            // Run initialization asynchronously after window is shown
            mainWindow.Loaded += async (_, _) =>
            {
                await InitializeAppAsync(desktop);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeAppAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            Debug.WriteLine("Starting application initialization...");

            // Step 1: Build AppPaths
            Debug.WriteLine("Initializing AppPaths...");
            _appPaths = new AppPaths();
            Debug.WriteLine($"Root: {_appPaths.Root}");
            Debug.WriteLine($"Data: {_appPaths.Data}");
            Debug.WriteLine($"Themes: {_appPaths.Themes}");
            Debug.WriteLine($"Logs: {_appPaths.Logs}");

            // Step 2: Load settings
            Debug.WriteLine("Loading settings...");
            _settingsStore = new SettingsStore(_appPaths);
            _settings = await _settingsStore.LoadAsync();
            Debug.WriteLine($"Settings loaded. FirstRunUtc: {_settings.FirstRunUtc}, LastRunUtc: {_settings.LastRunUtc}");

            // Step 3: Update LastRunUtc and save
            _settings.LastRunUtc = DateTime.UtcNow;
            await _settingsStore.SaveAsync(_settings);
            Debug.WriteLine("Settings saved with updated LastRunUtc.");

            // Step 3.5: Initialize and apply theme
            Debug.WriteLine("Initializing theme service...");
            _themeService = new ThemeService(this, _settingsStore, _settings);
            
            // Apply theme from settings (or default if empty)
            var themeId = string.IsNullOrWhiteSpace(_settings.ActiveThemeId) ? "Light" : _settings.ActiveThemeId;
            _themeService.ApplyTheme(themeId);
            Debug.WriteLine($"Theme applied: {themeId}");

            // Step 4: Bootstrap database asynchronously
            Debug.WriteLine("Starting database bootstrap...");
            _dbBootstrapper = new DatabaseBootstrapper(_appPaths);
            var dbResult = await _dbBootstrapper.BootstrapAsync();

            if (!dbResult.Success)
            {
                await ShowErrorAndExitAsync(desktop, $"Database initialization failed:\n{dbResult.ErrorMessage}");
                return;
            }

            Debug.WriteLine("Database bootstrap completed successfully.");
            Debug.WriteLine("Application initialization complete.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Initialization error: {ex}");
            await ShowErrorAndExitAsync(desktop, $"Application initialization failed:\n{ex.Message}");
        }
    }

    private async Task ShowErrorAndExitAsync(IClassicDesktopStyleApplicationLifetime desktop, string message)
    {
        Debug.WriteLine($"FATAL: {message}");

        if (desktop.MainWindow is not null)
        {
            var messageBox = new Window
            {
                Title = "Initialization Error",
                Width = 450,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Thickness(20),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };

            await messageBox.ShowDialog(desktop.MainWindow);
        }

        desktop.Shutdown(1);
    }
}
