using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileTransferino.Core.Models;
using FileTransferino.Data.Repositories;
using FileTransferino.Infrastructure;
using FileTransferino.Security;
using Microsoft.Extensions.Logging;

namespace FileTransferino.App.ViewModels;

/// <summary>
/// ViewModel for the Site Manager window.
/// </summary>
public sealed class SiteManagerViewModel(
    ISiteRepository siteRepository,
    ICredentialStore credentialStore,
    AppPaths appPaths,
    ILogger<SiteManagerViewModel>? logger
) : INotifyPropertyChanged
{
    // Store dependencies as readonly fields so they can be used by methods and future features
    private readonly ISiteRepository _siteRepository = siteRepository;
    private readonly ICredentialStore _credentialStore = credentialStore;
    private readonly AppPaths _appPaths = appPaths;
    private readonly ILogger<SiteManagerViewModel>? _logger = logger;

    private SiteProfile? _selectedSite;
    private string _name = string.Empty;
    private string _protocol = "FTP";
    private string _host = string.Empty;
    private int _port = 21;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _defaultRemotePath = "/";
    private string _defaultLocalPath = string.Empty;
    private bool _isPasswordChanged;

    public ObservableCollection<SiteProfile> Sites { get; } = new();

    public bool HasNoSites => Sites.Count == 0;

    public SiteProfile? SelectedSite
    {
        get => _selectedSite;
        set
        {
            if (_selectedSite == value)
                return;

            _selectedSite = value;
            OnPropertyChanged();
            LoadSelectedSite();
            // Update command availability when selection changes
            RaiseCommandCanExecuteChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            _name = value;
            OnPropertyChanged();
        }
    }

    public string Protocol
    {
        get => _protocol;
        set
        {
            if (_protocol == value)
                return;

            _protocol = value;
            OnPropertyChanged();
            UpdateDefaultPort();
        }
    }

    public string Host
    {
        get => _host;
        set
        {
            if (_host == value)
                return;

            _host = value;
            OnPropertyChanged();
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            if (_port == value)
                return;

            _port = value;
            OnPropertyChanged();
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            if (_username == value)
                return;

            _username = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password == value)
                return;

            _password = value;
            _isPasswordChanged = !string.IsNullOrEmpty(value);
            OnPropertyChanged();
        }
    }

    public string DefaultRemotePath
    {
        get => _defaultRemotePath;
        set
        {
            if (_defaultRemotePath == value)
                return;

            _defaultRemotePath = value;
            OnPropertyChanged();
        }
    }

    public string DefaultLocalPath
    {
        get => _defaultLocalPath;
        set
        {
            if (_defaultLocalPath == value)
                return;

            _defaultLocalPath = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadSitesAsync()
    {
        try
        {
            _logger?.LogInformation("Loading sites from repository");
            var sites = (await _siteRepository.GetAllAsync()).ToList();

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Sites.Clear();
                foreach (var site in sites)
                    Sites.Add(site);

                _logger?.LogInformation("Loaded {SiteCount} sites", Sites.Count);
                // Notify UI that site list presence may have changed
                OnPropertyChanged(nameof(HasNoSites));
                // Update command availability after loading
                RaiseCommandCanExecuteChanged();
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load sites from repository");
            throw;
        }
    }

    public void NewSite()
    {
        SelectedSite = null;
        Name = "New Site";
        Protocol = "FTP";
        Host = string.Empty;
        Port = 21;
        Username = string.Empty;
        Password = string.Empty;
        DefaultRemotePath = "/";
        DefaultLocalPath = string.Empty;
        _isPasswordChanged = false;
    }

    private void LoadSelectedSite()
    {
        if (SelectedSite == null)
        {
            // Initialize fields to default values without touching SelectedSite to avoid recursion
            Name = "New Site";
            Protocol = "FTP";
            Host = string.Empty;
            Port = 21;
            Username = string.Empty;
            Password = string.Empty;
            DefaultRemotePath = "/";
            DefaultLocalPath = string.Empty;
            _isPasswordChanged = false;
            return;
        }

        Name = SelectedSite.Name;
        Protocol = SelectedSite.Protocol;
        Host = SelectedSite.Host;
        Port = SelectedSite.Port;
        Username = SelectedSite.Username ?? string.Empty;
        DefaultRemotePath = SelectedSite.DefaultRemotePath ?? "/";
        DefaultLocalPath = SelectedSite.DefaultLocalPath ?? string.Empty;
        Password = string.Empty; // Never show saved password
        _isPasswordChanged = false;
    }

    public async Task<bool> SaveSiteAsync()
    {
        try
        {
            if (!ValidateSiteInputs()) return false;

            var site = PrepareSiteForSave();
            await SaveSiteToRepositoryAsync(site);
            UpdateUIAfterSave(site);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving site");
            return false;
        }
    }

    private bool ValidateSiteInputs()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Host))
        {
            _logger?.LogWarning("Cannot save site: Name or Host is empty");
            return false;
        }
        return true;
    }

    private SiteProfile PrepareSiteForSave()
    {
        var site = SelectedSite ?? new SiteProfile
        {
            Name = Name,
            Protocol = Protocol,
            Host = Host,
            Port = Port
        };

        site.Name = Name;
        site.Protocol = Protocol;
        site.Host = Host;
        site.Port = Port;
        site.Username = Username;
        site.DefaultRemotePath = DefaultRemotePath;
        site.DefaultLocalPath = DefaultLocalPath;

        if (_isPasswordChanged && !string.IsNullOrEmpty(Password))
        {
            if (string.IsNullOrEmpty(site.CredentialKey))
            {
                site.CredentialKey = $"site_{Guid.NewGuid():N}";
            }
            _credentialStore.SaveAsync(site.CredentialKey, Password);
        }

        return site;
    }

    private async Task SaveSiteToRepositoryAsync(SiteProfile site)
    {
        if (SelectedSite == null)
        {
            await _siteRepository.InsertAsync(site);
            Sites.Add(site);
            SelectedSite = site;
        }
        else
        {
            await _siteRepository.UpdateAsync(site);
            UpdateSiteInCollection(site);
        }
    }

    private void UpdateSiteInCollection(SiteProfile site)
    {
        var siteToUpdate = Sites.FirstOrDefault(s => s.Id == site.Id);
        if (siteToUpdate != null)
        {
            var index = Sites.IndexOf(siteToUpdate);
            if (index >= 0)
            {
                Sites[index] = site;
            }
        }
    }

    private void UpdateUIAfterSave(SiteProfile site)
    {
        Password = string.Empty;
        _isPasswordChanged = false;
        OnPropertyChanged(nameof(HasNoSites));
        RaiseCommandCanExecuteChanged();
    }

    public async Task<bool> DeleteSiteByIdAsync(int id)
    {
        try
        {
            var persisted = await _siteRepository.GetByIdAsync(id);
            if (persisted == null) return false;

            await DeleteSiteCredentialsAsync(persisted);
            await _siteRepository.DeleteAsync(id);
            UpdateUIAfterDelete(id);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting site");
            return false;
        }
    }

    private async Task DeleteSiteCredentialsAsync(SiteProfile site)
    {
        if (!string.IsNullOrEmpty(site.CredentialKey))
        {
            await _credentialStore.DeleteAsync(site.CredentialKey);
        }
    }

    private void UpdateUIAfterDelete(int id)
    {
        var siteToRemove = Sites.FirstOrDefault(s => s.Id == id);
        if (siteToRemove != null)
        {
            Sites.Remove(siteToRemove);
        }
        SelectedSite = Sites.FirstOrDefault();
        OnPropertyChanged(nameof(HasNoSites));
    }

    // Backwards-compatible: delete using selected site reference
    public Task<bool> DeleteSiteAsync()
        => DeleteSiteByIdAsync(SelectedSite?.Id ?? 0);

    private void UpdateDefaultPort()
    {
        Port = Protocol switch
        {
            "FTP" => 21,
            "FTPS" => 21,
            "SFTP" => 22,
            _ => 21
        };
    }

    // Simple command implementation for local use
    private sealed class DelegateCommand(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Action? _execute = execute;

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute?.Invoke();

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class AsyncDelegateCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncDelegateCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeAsync();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // Commands for the action bar
    private ICommand? _newSiteCommand;
    public ICommand NewSiteCommand => _newSiteCommand ??= new DelegateCommand(() => NewSite());

    private ICommand? _saveCommand;
    public ICommand SaveCommand => _saveCommand ??= new AsyncDelegateCommand(async () => await SaveSiteAsync(), () => true);

    private ICommand? _deleteCommand;
    public ICommand DeleteCommand => _deleteCommand ??= new AsyncDelegateCommand(async () => await DeleteSiteAsync(), () => SelectedSite != null);

    private ICommand? _openDocsCommand;
    public ICommand OpenDocsCommand => _openDocsCommand ??= new DelegateCommand(() =>
    {
        // For now, there is no dedicated online documentation URL for the Site Manager.
        // Log a clear message so that this is visible in diagnostics without misleading users.
        _logger?.LogInformation(
            "SiteManagerViewModel.OpenDocsCommand invoked, but no documentation URL is configured. " +
            "Please refer to the project README.md and solution-summary.md for current documentation.");
    });

    private ICommand? _connectCommand;
    public ICommand ConnectCommand => _connectCommand ??= new DelegateCommand(ConnectToSite, CanConnectToSite);

    // Ensure CanExecute changes when selection changes
    private void RaiseCommandCanExecuteChanged()
    {
        (_deleteCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (_saveCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (_newSiteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (_openDocsCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (_connectCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Added the ConnectToSite method to the ViewModel
    public void ConnectToSite()
    {
        // Logic to connect to the selected site
        System.Diagnostics.Debug.WriteLine(SelectedSite != null
            ? $"Connecting to site: {SelectedSite.Name}"
            : "No site selected to connect.");
    }

    private bool CanConnectToSite()
    {
        // Example: Enable ConnectCommand only if a site is selected
        return SelectedSite != null;
    }
}
