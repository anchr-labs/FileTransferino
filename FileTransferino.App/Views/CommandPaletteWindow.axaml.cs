using Avalonia.Controls;
using Avalonia.Input;
using FileTransferino.App.ViewModels;

namespace FileTransferino.App.Views;

public partial class CommandPaletteWindow : Window
{
    private readonly CommandPaletteViewModel _viewModel;

    public CommandPaletteWindow(CommandPaletteViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
        
        InitializeComponent();
        
        // Focus search box when window opens
        Opened += (_, _) =>
        {
            var searchBox = this.FindControl<TextBox>("SearchBox");
            searchBox?.Focus();
        };
        
        // Handle keyboard shortcuts
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        } else if (e.Key == Key.Enter)
        {
            _viewModel.ExecuteSelectedCommand();
            Close();
        }
    }
}
