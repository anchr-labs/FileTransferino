using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using FileTransferino.App.ViewModels;
using FileTransferino.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileTransferino.Core.Models;

namespace FileTransferino.App;

public partial class MainWindow : Window
{
    private SiteManagerViewModel? _siteManagerViewModel;
    private DispatcherTimer? _welcomeTimer;
    private bool _welcomeDismissed;
    private CommandPaletteWindow? _commandPaletteWindow; // Reuse single instance

    public MainWindow()
    {
        InitializeComponent();
        
        // Register keyboard shortcuts
        KeyDown += OnKeyDown;
        
        // Initialize Site Manager
        Opened += OnWindowOpened;
        
        // Clean up palette window when main window closes
        Closing += (_, _) =>
        {
            _commandPaletteWindow?.Close();
            _commandPaletteWindow = null;
        };
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
            await WaitForAppServicesInitialization(app);

            var siteManagerView = this.FindControl<SiteManagerView>("SiteManagerView");
            if (siteManagerView != null)
            {
                await SetupSiteManagerViewModel(app, siteManagerView);
            }

            await SeedDemoSiteIfNeededAsync(app, _siteManagerViewModel);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Site Manager: {ex}");
        }
    }

    private async Task WaitForAppServicesInitialization(App? app)
    {
        var attempts = 0;
        while ((app?.SiteRepository == null || app.CredentialStore == null || app.AppPaths == null || app.Services == null) && attempts < 100)
        {
            await Task.Delay(100);
            attempts++;
            app = Application.Current as App;
        }
    }

    private async Task SetupSiteManagerViewModel(App? app, SiteManagerView siteManagerView)
    {
        int attempts = 0; // Ensure attempts is initialized

        if (app?.SiteRepository != null && app.CredentialStore != null && app.AppPaths != null && app.Services != null)
        {
            if (siteManagerView.DataContext == null)
            {
                var logger = app.Services.GetService<ILogger<SiteManagerViewModel>>();
                _siteManagerViewModel = new SiteManagerViewModel(app.SiteRepository, app.CredentialStore, app.AppPaths, logger);
                siteManagerView.DataContext = _siteManagerViewModel;
                // Load sites with the established site repository
                await _siteManagerViewModel.LoadSitesAsync();
                File.AppendAllText("debug_cmd_states.log", $"{DateTime.Now:O} MainWindow set DataContext after {attempts} attempts\n");
            }
            else
            {
                File.AppendAllText("debug_cmd_states.log", $"{DateTime.Now:O} MainWindow found existing DataContext (view fallback)\n");
            }
        }
        else
        {
            File.AppendAllText("debug_cmd_states.log", $"{DateTime.Now:O} MainWindow gave up waiting for App services after {attempts} attempts\n");
        }

        // Ensure we have a VM reference (maybe set by the view fallback)
        if (siteManagerView != null)
            _siteManagerViewModel ??= siteManagerView.DataContext as SiteManagerViewModel;

        // Load sites if we have a ViewModel
        if (_siteManagerViewModel != null)
            await _siteManagerViewModel.LoadSitesAsync();
    }

    /// <summary>
    /// Seed a demo site when no sites exist (first-run friendly). This is safe and idempotent.
    /// This method encapsulates data initialization concerns away from the UI initialization flow.
    /// </summary>
    /// <param name="app">The current application instance providing access to the site repository.</param>
    /// <param name="siteManagerViewModel">The site manager view model used to inspect and reload sites.</param>
    private static async Task SeedDemoSiteIfNeededAsync(App? app, SiteManagerViewModel? siteManagerViewModel)
    {
        try
        {
            if (app?.SiteRepository != null && siteManagerViewModel != null && siteManagerViewModel.Sites.Count == 0)
            {
                var demo = new SiteProfile
                {
                    Name = "Demo FTP",
                    Protocol = "FTP",
                    Host = "ftp.example.com",
                    Port = 21,
                    Username = "anonymous",
                    DefaultRemotePath = "/",
                    DefaultLocalPath = string.Empty
                };

                var id = await app.SiteRepository.InsertAsync(demo);
                demo.Id = id;
                System.IO.File.AppendAllText(@"C:\dev-priv\FileTransferino\debug_cmd_states.log", $"{DateTime.Now:O} Demo site inserted with ID {id}\n");

                // Reload the VM's sites so the UI updates
                await siteManagerViewModel.LoadSitesAsync();
            }
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(@"C:\dev-priv\FileTransferino\debug_cmd_states.log", $"{DateTime.Now:O} Demo seed failed: {ex.Message}\n");
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
            // First run: show overlay for 4 seconds
            overlay.IsVisible = true;
            _welcomeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
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

        // Ctrl+Space to open command palette
        if (e.Key == Key.Space && e.KeyModifiers.HasFlag(KeyModifiers.Control))
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

        // Create fresh ViewModel each time with current theme state
        var viewModel = new CommandPaletteViewModel(themeService, themeService.CurrentThemeId);

        // Register all theme commands at once (batch registration for proper selection)
        var themeCommands = themeService.GetThemes().Select(theme => new PaletteCommand
        {
            Name = theme.DisplayName,
            Category = "Themes", // Place themes in the outer menu
            Id = theme.Id,
            Action = () => themeService.ApplyTheme(theme.Id)
        }).ToList();
        
        viewModel.RegisterCommands(themeCommands);

        // Reuse window instance but refresh ViewModel to reset state
        if (_commandPaletteWindow == null)
        {
            _commandPaletteWindow = new CommandPaletteWindow(viewModel);
        }
        else
        {
            // Update the DataContext to the fresh ViewModel
            _commandPaletteWindow.DataContext = viewModel;
            // Reset window state (search text, selection, etc.) will happen via new VM
        }
        
        // Show modeless so applying themes or focusing doesn't minimize the owner window
        _commandPaletteWindow.Show();
        _commandPaletteWindow.Activate();

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

    private void OpenSiteManager()
    {
        // Placeholder implementation for opening the Site Manager
        // Ensure this method is properly implemented
        var siteManagerWindow = new SiteManagerWindow();
        siteManagerWindow.ShowDialog(this);
    }
}
