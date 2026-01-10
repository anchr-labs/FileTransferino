using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using FileTransferino.App.Services;

namespace FileTransferino.App.ViewModels;

/// <summary>
/// Represents a command in the command palette.
/// </summary>
public sealed class PaletteCommand
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required Action Action { get; init; }
    // Optional id (used for themes)
    public string? Id { get; init; }
}

/// <summary>
/// ViewModel for the command palette.
/// </summary>
public sealed class CommandPaletteViewModel : INotifyPropertyChanged
{
    private string _searchText = string.Empty;
    private PaletteCommand? _selectedCommand;
    private readonly List<PaletteCommand> _allCommands = new();

    public ObservableCollection<PaletteCommand> FilteredCommands { get; } = new();

    // Theme preview support
    private readonly IThemeService? _themeService;
    private readonly string? _originalThemeId;
    private CancellationTokenSource? _previewCts;
    private readonly int _debounceMs = 50; // faster preview response

    public CommandPaletteViewModel() { }

    public CommandPaletteViewModel(IThemeService themeService, string? originalThemeId, int debounceMilliseconds = 50)
    {
        _themeService = themeService;
        _originalThemeId = originalThemeId;
        _debounceMs = debounceMilliseconds;
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged();
            FilterCommands();
        }
    }

    public PaletteCommand? SelectedCommand
    {
        get => _selectedCommand;
        set
        {
            if (_selectedCommand != value)
            {
                _selectedCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public void RegisterCommand(PaletteCommand command)
    {
        _allCommands.Add(command);
        FilterCommands();
    }

    public void RegisterCommands(IEnumerable<PaletteCommand> commands)
    {
        _allCommands.AddRange(commands);
        FilterCommands();
    }

    public void ClearCommands()
    {
        _allCommands.Clear();
        FilteredCommands.Clear();
    }

    private void FilterCommands()
    {
        FilteredCommands.Clear();

        var query = _searchText.Trim().ToLowerInvariant();
        
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allCommands
            : _allCommands.Where(c => 
                c.Name.ToLowerInvariant().Contains(query) || 
                c.Category.ToLowerInvariant().Contains(query));

        foreach (var command in filtered)
        {
            FilteredCommands.Add(command);
        }

        // Auto-select first command
        SelectedCommand = FilteredCommands.FirstOrDefault();
    }

    public void ExecuteSelectedCommand()
    {
        // Cancel any pending preview to avoid race
        CancelPendingPreview();
        SelectedCommand?.Action.Invoke();
    }

    /// <summary>
    /// Preview a theme (or other command) immediately. Used for selection changes.
    /// </summary>
    public void PreviewCommand(PaletteCommand? command)
    {
        if (command == null)
            return;

        // Only preview if it's a theme command (has Id and ThemeService)
        // Do NOT execute non-theme commands on selection change
        if (_themeService != null && !string.IsNullOrEmpty(command.Id))
        {
            CancelPendingPreview();
            // Apply immediately on UI thread
            Dispatcher.UIThread.Post(() => _themeService.ApplyTheme(command.Id!));
        }
        
        // Non-theme commands should NOT be executed on selection change
        // They should only execute on Enter or Click
    }

    /// <summary>
    /// Preview a theme with debounce. Used for hover preview to avoid spamming.
    /// </summary>
    public void PreviewCommandDebounced(PaletteCommand? command)
    {
        if (command == null)
            return;

        // If we have a theme service and an id, use debounced preview
        if (_themeService != null && !string.IsNullOrEmpty(command.Id))
        {
            DebouncedApply(command.Id!);
            return;
        }
    }

    private void DebouncedApply(string themeId)
    {
        CancelPendingPreview();
        _previewCts = new CancellationTokenSource();
        var ct = _previewCts.Token;

        // Fire-and-forget async debounce without blocking UI
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_debounceMs, ct);
                if (ct.IsCancellationRequested) return;

                // Apply on UI thread via theme service (ApplyTheme may interact with Application resources)
                await Dispatcher.UIThread.InvokeAsync(() => _themeService?.ApplyTheme(themeId));
            }
            catch (OperationCanceledException)
            {
                // Expected when preview is cancelled, no logging needed
            }
            catch (Exception ex)
            {
                // Log unexpected exceptions during theme preview with full details
                Debug.WriteLine($"Error during theme preview: {ex}");
            }
        }, ct);
    }

    private void CancelPendingPreview()
    {
        try
        {
            _previewCts?.Cancel();
            _previewCts?.Dispose();
            _previewCts = null;
        }
        catch { }
    }

    /// <summary>
    /// Restore the theme that was active when the palette opened.
    /// </summary>
    public void RestoreOriginalTheme()
    {
        CancelPendingPreview();
        if (_themeService != null && !string.IsNullOrEmpty(_originalThemeId))
        {
            _themeService.ApplyTheme(_originalThemeId!);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
