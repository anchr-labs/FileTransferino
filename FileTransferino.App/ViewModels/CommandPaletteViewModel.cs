using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FileTransferino.App.ViewModels;

/// <summary>
/// Represents a command in the command palette.
/// </summary>
public sealed class PaletteCommand
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required Action Action { get; init; }
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

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                FilterCommands();
            }
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
        SelectedCommand?.Action.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
