using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using FileTransferino.App.ViewModels;

namespace FileTransferino.App.Views;

public partial class CommandPaletteWindow : Window
{
    private readonly CommandPaletteViewModel _viewModel;
    private ListBox? _list;
    private TextBox? _searchBox;
    private bool _selectionConfirmed;

    /// <summary>
    /// Parameterless constructor required by the XAML loader and design-time tools.
    /// </summary>
    /// <remarks>
    /// This overload creates a <see cref="CommandPaletteViewModel"/> without the theme service,
    /// so theme preview functionality will not work if this constructor is used at runtime.
    /// Application code should prefer the constructor that accepts a preconfigured
    /// <see cref="CommandPaletteViewModel"/> with the required services.
    /// </remarks>
    public CommandPaletteWindow() : this(new CommandPaletteViewModel()) { }

    public CommandPaletteWindow(CommandPaletteViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
        
        InitializeComponent();
        
        _searchBox = this.FindControl<TextBox>("SearchBox");
        _list = this.FindControl<ListBox>("CommandList");
        
        // Focus search box when window opens
        Opened += (_, _) =>
        {
            _searchBox?.Focus();
        };
        
        // Close when clicking outside or losing focus
        Deactivated += (_, _) =>
        {
            // Restore original theme when closing without confirming
            if (!_selectionConfirmed)
            {
                _viewModel.RestoreOriginalTheme();
            }
            Close();
        };
        
        // Handle keyboard shortcuts at window level
        KeyDown += OnKeyDown;

        // Handle arrow navigation from search box to list
        if (_searchBox != null)
        {
            _searchBox.KeyDown += OnSearchBoxKeyDown;
        }

        // Wire up list interactions
        if (_list != null)
        {
            // Selection changes should preview the theme in main window
            _list.SelectionChanged += (_, _) =>
            {
                var cmd = _viewModel.SelectedCommand;
                if (cmd != null)
                {
                    _viewModel.PreviewCommand(cmd);
                }
            };

            // Single left-click applies immediately but keep dialog open
            _list.DoubleTapped += (_, _) =>
            {
                var cmd = _viewModel.SelectedCommand;
                cmd?.Action.Invoke();
            };
            
            // Also handle single click on items
            _list.PointerPressed += OnListPointerPressed;
            
            // Hover preview: when mouse moves over an item, preview that theme
            _list.PointerMoved += OnListPointerMoved;
        }
    }

    private void OnListPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_list == null) return;
        
        // Find which item is under the pointer
        var point = e.GetPosition(_list);
        var element = _list.InputHitTest(point) as Control;
        
        // Walk up the visual tree to find the ListBoxItem
        while (element != null && element is not ListBoxItem)
        {
            element = element.Parent as Control;
        }
        
        if (element is ListBoxItem listBoxItem && listBoxItem.DataContext is PaletteCommand cmd)
        {
            // Preview the theme under the cursor with debounce to avoid spam
            _viewModel.PreviewCommandDebounced(cmd);
        }
    }

    private void OnSearchBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (_list == null || _viewModel.FilteredCommands.Count == 0)
            return;

        if (e.Key == Key.Down)
        {
            e.Handled = true;

            // Move selection down
            var currentIndex = _list.SelectedIndex;
            if (currentIndex < _viewModel.FilteredCommands.Count - 1)
            {
                _list.SelectedIndex = currentIndex + 1;
            }
            else if (currentIndex == -1)
            {
                _list.SelectedIndex = 0;
            }

            // Focus the list so arrow keys continue to work
            _list.Focus();
        }
        else if (e.Key == Key.Up)
        {
            e.Handled = true;

            // Move selection up
            var currentIndex = _list.SelectedIndex;
            if (currentIndex > 0)
            {
                _list.SelectedIndex = currentIndex - 1;
            }

            // Focus the list
            _list.Focus();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            // Restore theme to original state if possible
            _viewModel.RestoreOriginalTheme();
            Close();
        }
        else if (e.Key == Key.Enter)
        {
            _selectionConfirmed = true;
            _viewModel.ExecuteSelectedCommand();
            Close();
        }
    }

    private void OnListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Only handle left-button presses
        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed) 
            return;

        // Ignore clicks that originate from scroll bar controls
        var src = e.Source;
        if (src is ScrollBar || src is Thumb || src is RepeatButton)
            return;

        // Apply immediately on click (selection will update via SelectionChanged first)
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var cmd = _viewModel.SelectedCommand;
            if (cmd != null)
            {
                // Mark as confirmed so theme isn't restored on close
                _selectionConfirmed = true;
                // Cancel pending previews and execute immediately
                _viewModel.ExecuteSelectedCommand();
            }
        }, DispatcherPriority.Background);
    }
}
