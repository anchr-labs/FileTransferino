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

            // Set DataContext for the embedded SiteManagerView
            var siteManagerView = this.FindControl<SiteManagerView>("SiteManagerView");
            if (siteManagerView != null)
            {
                // Wait for App services to initialize (up to 10 seconds)
                var attempts = 0;
                while (!AreServicesInitialized(app) && attempts < 100)
                {
                    await Task.Delay(100);
                    attempts++;
                    app = Application.Current as App;
                }

                if (AreServicesInitialized(app))
                {
                    // If the view didn't already set a DataContext (fallback), create and assign the primary VM
                    if (siteManagerView.DataContext == null)
                    {
                        var logger = app.Services.GetService<ILogger<SiteManagerViewModel>>();
                        _siteManagerViewModel = new SiteManagerViewModel(app.SiteRepository, app.CredentialStore, app.AppPaths, logger);
                        siteManagerView.DataContext = _siteManagerViewModel;
                        // Load sites with the established site repository
                        await _siteManagerViewModel.LoadSitesAsync();
                        System.IO.File.AppendAllText(@"C:\dev-priv\FileTransferino\debug_cmd_states.log", $"{DateTime.Now:O} MainWindow set DataContext after {attempts} attempts\n");
                    }
                    else
                    {
                        System.IO.File.AppendAllText(@"C:\dev-priv\FileTransferino\debug_cmd_states.log", $"{DateTime.Now:O} MainWindow found existing DataContext (view fallback)\n");
                    }
                }
                else
                {
                    System.IO.File.AppendAllText(@"C:\dev-priv\FileTransferino\debug_cmd_states.log", $"{DateTime.Now:O} MainWindow gave up waiting for App services after {attempts} attempts\n");
                }
            }

            // Ensure we have a VM reference (maybe set by the view fallback)
            if (siteManagerView != null)
                _siteManagerViewModel ??= siteManagerView.DataContext as SiteManagerViewModel;

            // Load sites if we have a ViewModel
            if (_siteManagerViewModel != null)
                await _siteManagerViewModel.LoadSitesAsync();

            // Seed a demo site when no sites exist (first-run friendly). This is safe and idempotent
            try
            {
                if (app?.SiteRepository != null && _siteManagerViewModel != null && _siteManagerViewModel.Sites.Count == 0)
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
                    await _siteManagerViewModel.LoadSitesAsync();
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(@"C:\dev-priv\FileTransferino\debug_cmd_states.log", $"{DateTime.Now:O} Demo seed failed: {ex.Message}\n");
            }
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

    private static bool AreServicesInitialized(App? app)
    {
        return app?.SiteRepository != null 
            && app.CredentialStore != null 
            && app.AppPaths != null 
            && app.Services != null;
    }
}
