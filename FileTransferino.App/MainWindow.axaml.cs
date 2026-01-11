using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileTransferino.App.ViewModels;
using FileTransferino.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTransferino.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Register Ctrl+K to open command palette
        KeyDown += OnKeyDown;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+K to open command palette
        if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            await OpenCommandPalette();
        }
    }

    private async Task OpenCommandPalette()
    {
        var app = Application.Current as App;
        var themeService = app?.ThemeService;
        
        if (themeService == null)
            return;

        // Pass themeService and current theme id for preview/restore support
        var viewModel = new CommandPaletteViewModel(themeService, themeService.CurrentThemeId);
        
        // Register theme submenu as a single top-level entry
        var themeCommands = new List<PaletteCommand>();
        foreach (var theme in themeService.GetThemes())
        {
            var themeId = theme.Id; // Capture for closure
            themeCommands.Add(new PaletteCommand
            {
                Name = theme.DisplayName,
                Category = "Theme",
                Id = themeId,
                Action = () => themeService.ApplyTheme(themeId)
            });
        }

        // Top-level 'Themes' entry opens the submenu
        viewModel.RegisterCommand(new PaletteCommand
        {
            Name = "Themes...",
            Category = "Theme",
            Action = () => viewModel.EnterSubmenu("Themes", themeCommands)
        });
        
        // Register Site Manager command
        viewModel.RegisterCommand(new PaletteCommand
        {
            Name = "Open Site Manager",
            Category = "Sites",
            Action = OpenSiteManager
        });
        
        var paletteWindow = new CommandPaletteWindow(viewModel);
        paletteWindow.Show(); // Use Show instead of ShowDialog so main window remains visible for preview
        await Task.CompletedTask;
    }

    private async void OpenSiteManager()
    {
        try
        {
            var app = Application.Current as App;
            if (app?.SiteRepository == null || app.CredentialStore == null || app.AppPaths == null || app.Services == null)
                return;

            // Ensure the persisted/confirmed theme is applied before opening the Site Manager
            app.ThemeService?.RestoreActiveTheme();

            var logger = app.Services.GetService<ILogger<SiteManagerViewModel>>();

            var viewModel = new SiteManagerViewModel(app.SiteRepository, app.CredentialStore, app.AppPaths, logger);
            var siteManager = new SiteManagerWindow(viewModel);
            await siteManager.ShowDialog(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening Site Manager: {ex}");
        }
    }

    private void OnTitleBarPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        // Allow dragging the window by the title bar
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    private void CloseWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
