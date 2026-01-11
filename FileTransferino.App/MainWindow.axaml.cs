using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using FileTransferino.App.ViewModels;
using FileTransferino.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTransferino.App;

public partial class MainWindow : Window
{
    private SiteManagerViewModel? _siteManagerViewModel;
    private DispatcherTimer? _welcomeTimer;
    private bool _welcomeDismissed = false;

    public MainWindow()
    {
        InitializeComponent();
        
        // Register keyboard shortcuts
        KeyDown += OnKeyDown;
        
        // Initialize Site Manager
        Opened += OnWindowOpened;
    }

    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        await InitializeSiteManager();
        ShowWelcomeOverlay();
    }

    private async Task InitializeSiteManager()
    {
        try
        {
            var app = Application.Current as App;
            if (app?.SiteRepository == null || app.CredentialStore == null || app.AppPaths == null || app.Services == null)
                return;

            var logger = app.Services.GetService<ILogger<SiteManagerViewModel>>();
            _siteManagerViewModel = new SiteManagerViewModel(app.SiteRepository, app.CredentialStore, app.AppPaths, logger);
            
            // Set DataContext for the embedded SiteManagerView
            var siteManagerView = this.FindControl<SiteManagerView>("SiteManagerView");
            if (siteManagerView != null)
            {
                siteManagerView.DataContext = _siteManagerViewModel;
            }
            
            // Load sites
            await _siteManagerViewModel.LoadSitesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Site Manager: {ex}");
        }
    }

    private void ShowWelcomeOverlay()
    {
        var overlay = this.FindControl<Border>("WelcomeOverlay");
        if (overlay == null) return;

        // Check if this is first run (no sites exist)
        var hasExistingSites = _siteManagerViewModel?.Sites?.Count > 0;
        
        if (!hasExistingSites)
        {
            // First run: show overlay for 5 seconds
            overlay.IsVisible = true;
            _welcomeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _welcomeTimer.Tick += (s, e) => DismissWelcomeOverlay();
            _welcomeTimer.Start();
        }
        // Subsequent runs: no overlay
    }

    private void DismissWelcomeOverlay()
    {
        if (_welcomeDismissed) return;
        
        _welcomeDismissed = true;
        _welcomeTimer?.Stop();
        
        var overlay = this.FindControl<Border>("WelcomeOverlay");
        if (overlay != null)
        {
            overlay.IsVisible = false;
        }
    }

    private void OnWelcomeOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        DismissWelcomeOverlay();
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Dismiss welcome overlay on any key press
        if (!_welcomeDismissed)
        {
            DismissWelcomeOverlay();
            e.Handled = true;
            return;
        }

        // Ctrl+K to open command palette
        if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            await OpenCommandPalette();
        }
        // Ctrl+N for New Site
        else if (e.Key == Key.N && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            _siteManagerViewModel?.NewSite();
        }
        // Ctrl+S for Save Site
        else if (e.Key == Key.S && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            await (_siteManagerViewModel?.SaveSiteAsync() ?? Task.CompletedTask);
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
        
        var paletteWindow = new CommandPaletteWindow(viewModel);
        paletteWindow.Show(); // Use Show instead of ShowDialog so main window remains visible for preview
        await Task.CompletedTask;
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
