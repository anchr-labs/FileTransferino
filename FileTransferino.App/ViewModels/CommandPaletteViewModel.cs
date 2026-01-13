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
    public string? Id { get; init; }
}

/// <summary>
/// ViewModel for the command palette.
/// </summary>
/// <param name="themeService">Theme service for preview functionality (null disables previews)</param>
/// <param name="originalThemeId">Theme ID to restore on cancel (null skips restoration)</param>
/// <param name="debounceMilliseconds">Debounce delay for hover previews</param>
public sealed class CommandPaletteViewModel(
    IThemeService? themeService = null,
    string? originalThemeId = null,
    int debounceMilliseconds = 200) : INotifyPropertyChanged
{
    private string _searchText = string.Empty;
    private PaletteCommand? _selectedCommand;
    private CancellationTokenSource? _previewCts;

    // Root command list (top-level)
    private readonly List<PaletteCommand> _rootCommands = [];

    // Submenu state
    private List<PaletteCommand>? _submenuCommands;
    private bool _inSubmenu;
    private string? _submenuTitle;

    // Active command list (points to root or submenu)
    private List<PaletteCommand> ActiveCommands => _inSubmenu && _submenuCommands != null 
        ? _submenuCommands 
        : _rootCommands;

    public ObservableCollection<PaletteCommand> FilteredCommands { get; } = [];

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

    private string? _lastVisitedThemeId;

    public PaletteCommand? SelectedCommand
    {
        get => _selectedCommand;
        set
        {
            if (_selectedCommand != value)
            {
                _selectedCommand = value;
                // Track last visited theme id so we can restore selection when re-opening submenus
                if (_selectedCommand?.Id != null)
                {
                    _lastVisitedThemeId = _selectedCommand.Id;
                    // persist across sessions if themeService supports it
                    try { themeService?.LastVisitedThemeId = _selectedCommand.Id; } catch { }
                }
                OnPropertyChanged();
            }
        }
    }

    public bool InSubmenu => _inSubmenu;
    public string? SubmenuTitle => _submenuTitle;

    public void RegisterCommand(PaletteCommand command)
    {
        _rootCommands.Add(command);
        FilterCommands();
    }

    public void RegisterCommands(IEnumerable<PaletteCommand> commands)
    {
        _rootCommands.AddRange(commands);
        FilterCommands();
    }

    public void ClearCommands()
    {
        _rootCommands.Clear();
        _submenuCommands = null;
        _inSubmenu = false;
        FilteredCommands.Clear();
    }

    /// <summary>
    /// Enter a submenu (replace the visible commands). Call ExitSubmenu() to return.
    /// </summary>
    public void EnterSubmenu(string title, IEnumerable<PaletteCommand> commands)
    {
        _submenuTitle = title;
        _submenuCommands = commands.ToList();
        _inSubmenu = true;
        FilterCommands();

        // After filling FilteredCommands, attempt to pre-select the best candidate:
        // 1. previously visited theme (_lastVisitedThemeId)
        // 2. current applied theme (from themeService)
        // 3. fallback to first item (already handled by FilterCommands)
        var desiredId = themeService?.LastVisitedThemeId ?? _lastVisitedThemeId;
        if (string.IsNullOrEmpty(desiredId) && themeService != null)
        {
            desiredId = themeService.CurrentThemeId;
        }

        if (!string.IsNullOrEmpty(desiredId))
        {
            var match = FilteredCommands.FirstOrDefault(c => c.Id == desiredId);
            if (match != null)
            {
                SelectedCommand = match;
            }
        }
    }

    /// <summary>
    /// Exit current submenu and restore top-level commands.
    /// </summary>
    public void ExitSubmenu()
    {
        if (!_inSubmenu)
            return;

        _submenuCommands = null;
        _submenuTitle = null;
        _inSubmenu = false;
        FilterCommands();
    }

    private void FilterCommands()
    {
        FilteredCommands.Clear();

        var query = _searchText.Trim().ToLowerInvariant();
        var source = ActiveCommands;

        if (source.Count == 0)
            return;

        var filtered = string.IsNullOrWhiteSpace(query)
            ? source
            : source.Where(c =>
                c.Name.ToLowerInvariant().Contains(query) ||
                c.Category.ToLowerInvariant().Contains(query));

        foreach (var command in filtered)
            FilteredCommands.Add(command);

        // Try to restore the last visited theme selection, otherwise select first item
        PaletteCommand? itemToSelect = null;
        
        // First, try to find the last visited theme
        var desiredId = themeService?.LastVisitedThemeId ?? _lastVisitedThemeId;
        if (!string.IsNullOrEmpty(desiredId))
        {
            itemToSelect = FilteredCommands.FirstOrDefault(c => c.Id == desiredId);
        }
        
        // If no last visited theme, try current active theme
        if (itemToSelect == null && themeService != null && !string.IsNullOrEmpty(themeService.CurrentThemeId))
        {
            itemToSelect = FilteredCommands.FirstOrDefault(c => c.Id == themeService.CurrentThemeId);
        }
        
        // Fallback to first item
        itemToSelect ??= FilteredCommands.FirstOrDefault();
        
        SelectedCommand = itemToSelect;
    }

    public void ExecuteSelectedCommand()
    {
        CancelPendingPreview();
        SelectedCommand?.Action.Invoke();
    }

    /// <summary>
    /// Preview a theme immediately. Used for selection changes.
    /// </summary>
    public void PreviewCommand(PaletteCommand? command)
    {
        if (command == null || themeService == null || string.IsNullOrEmpty(command.Id))
            return;

        CancelPendingPreview();
        Dispatcher.UIThread.Post(() => themeService.PreviewTheme(command.Id!));
    }

    /// <summary>
    /// Preview a theme with debounce. Used for hover preview.
    /// </summary>
    public void PreviewCommandDebounced(PaletteCommand? command)
    {
        if (command == null || themeService == null || string.IsNullOrEmpty(command.Id))
            return;

        DebouncedApplyPreview(command.Id!);
    }

    private void DebouncedApplyPreview(string themeId)
    {
        CancelPendingPreview();
        _previewCts = new CancellationTokenSource();
        var ct = _previewCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(debounceMilliseconds, ct);
                if (ct.IsCancellationRequested) return;

                await Dispatcher.UIThread.InvokeAsync(() => themeService?.PreviewTheme(themeId));
            }
            catch (OperationCanceledException)
            {
                // Expected when preview is cancelled
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during theme preview (themeId: {themeId}): {ex}");
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
        catch
        {
            // Ignore disposal errors
        }
    }

    /// <summary>
    /// Restore the theme that was active when the palette opened.
    /// </summary>
    public void RestoreOriginalTheme()
    {
        CancelPendingPreview();
        if (themeService != null && !string.IsNullOrEmpty(originalThemeId))
        {
            themeService.ApplyTheme(originalThemeId);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
