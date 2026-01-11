using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileTransferino.App.Services;
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
        
        // Register theme commands
        foreach (var theme in themeService.GetThemes())
        {
            var themeId = theme.Id; // Capture for closure
            viewModel.RegisterCommand(new PaletteCommand
            {
                Name = theme.DisplayName,
                Category = "Theme",
                Id = themeId,
                Action = () => themeService.ApplyTheme(themeId)
            });
        }
        
        // Register Site Manager command
        viewModel.RegisterCommand(new PaletteCommand
        {
            Name = "Open Site Manager",
            Category = "Sites",
            Action = () => OpenSiteManager()
        });
        
        var paletteWindow = new CommandPaletteWindow(viewModel);
        paletteWindow.Show(); // Use Show instead of ShowDialog so main window remains visible for preview
        await Task.CompletedTask;
    }

    private async void OpenSiteManager()
    {
        var app = Application.Current as App;
        if (app?.SiteRepository == null || app?.CredentialStore == null || app?.Services == null)
            return;

        var logger = app.Services.GetService<ILogger<SiteManagerViewModel>>();
        
        var viewModel = new SiteManagerViewModel(app.SiteRepository, app.CredentialStore, logger);
        var siteManager = new SiteManagerWindow(viewModel);
        await siteManager.ShowDialog(this);
    }
}
