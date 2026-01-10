using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileTransferino.App.Services;
using FileTransferino.App.ViewModels;
using FileTransferino.App.Views;

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

        var viewModel = new CommandPaletteViewModel();
        
        // Register theme commands
        foreach (var theme in themeService.GetThemes())
        {
            var themeId = theme.Id; // Capture for closure
            viewModel.RegisterCommand(new PaletteCommand
            {
                Name = theme.DisplayName,
                Category = "Theme",
                Action = () => themeService.ApplyTheme(themeId)
            });
        }
        
        var paletteWindow = new CommandPaletteWindow(viewModel);
        await paletteWindow.ShowDialog(this);
    }
}
