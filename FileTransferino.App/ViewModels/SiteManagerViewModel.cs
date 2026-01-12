using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileTransferino.Core.Models;
using FileTransferino.Data.Repositories;
using FileTransferino.Infrastructure;
using FileTransferino.Security;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace FileTransferino.App.ViewModels;

/// <summary>
/// ViewModel for the Site Manager window.
/// </summary>
public sealed class SiteManagerViewModel(
    ISiteRepository siteRepository,
    ICredentialStore credentialStore,
    AppPaths appPaths,
    ILogger<SiteManagerViewModel> logger
) : INotifyPropertyChanged
{
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

    public ObservableCollection<SiteProfile> Sites { get; } = [];

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
            logger?.LogInformation("Loading sites from repository");
            var sites = (await siteRepository.GetAllAsync()).ToList();

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Sites.Clear();
                foreach (var site in sites)
                    Sites.Add(site);

                logger?.LogInformation("Loaded {SiteCount} sites", Sites.Count);
                // Notify UI that site list presence may have changed
                OnPropertyChanged(nameof(HasNoSites));
                // Update command availability after loading
                RaiseCommandCanExecuteChanged();
            });
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to load sites from repository");
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
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Host))
        {
            logger?.LogWarning("Cannot save site: Name or Host is empty");
            return false;
        }

        try
        {
            var isNew = SelectedSite == null;
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

            // Handle password update
            if (_isPasswordChanged && !string.IsNullOrEmpty(Password))
            {
                // Generate credential key if needed
                if (string.IsNullOrEmpty(site.CredentialKey))
                {
                    site.CredentialKey = $"site_{Guid.NewGuid():N}";
                }

                // Save encrypted password
                await credentialStore.SaveAsync(site.CredentialKey, Password);
                logger?.LogInformation("Saved credentials for site {SiteName} with key {CredentialKey}", site.Name, site.CredentialKey);
            }

            if (isNew)
            {
                await siteRepository.InsertAsync(site);
                Sites.Add(site);
                SelectedSite = site;
                logger?.LogInformation("Created new site {SiteName} with ID {SiteId}", site.Name, site.Id);
            }
            else
            {
                await siteRepository.UpdateAsync(site);
                
                // Update the item in the list by Id to avoid null-reference warnings
                var index = -1;
                for (var i = 0; i < Sites.Count; i++)
                {
                    if (Sites[i].Id == site.Id)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    Sites[index] = site;
                    SelectedSite = site;
                }

                logger?.LogInformation("Updated site {SiteName} with ID {SiteId}", site.Name, site.Id);
            }

            Password = string.Empty;
            _isPasswordChanged = false;
            // Notify UI that site list presence may have changed
            OnPropertyChanged(nameof(HasNoSites));
            // Update command availability after save
            RaiseCommandCanExecuteChanged();
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to save site {SiteName}", Name);
            return false;
        }
    }

    /// <summary>
    /// Safely delete a site by id. Re-fetches the persisted record to obtain credential key
    /// and performs DB + credential deletion. Updates the UI collection on the UI thread.
    /// </summary>
    public async Task<bool> DeleteSiteByIdAsync(int id)
    {
        logger?.LogInformation("Starting site deletion for ID {SiteId}", id);

        try
        {
            // Re-fetch persisted site to get up-to-date credential key
            var persisted = await siteRepository.GetByIdAsync(id);
            if (persisted == null)
            {
                logger?.LogWarning("Site with ID {SiteId} not found in database", id);
                return false;
            }

            logger?.LogDebug("Fetched site {SiteName} (ID: {SiteId}) with credential key: {CredentialKey}",
                persisted.Name, persisted.Id, persisted.CredentialKey ?? "(null)");

            // Delete credential if present
            if (!string.IsNullOrEmpty(persisted.CredentialKey))
            {
                try
                {
                    await credentialStore.DeleteAsync(persisted.CredentialKey);
                    logger?.LogInformation("Deleted credentials for site {SiteName} with key {CredentialKey}",
                        persisted.Name, persisted.CredentialKey);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to delete credentials for site {SiteName} with key {CredentialKey}",
                        persisted.Name, persisted.CredentialKey);
                    // Continue with site deletion even if credential deletion fails
                }
            }

            // Delete from DB
            var deleted = await siteRepository.DeleteAsync(id);
            if (!deleted)
            {
                logger?.LogError("Failed to delete site {SiteName} (ID: {SiteId}) from database", persisted.Name, id);
                return false;
            }

            logger?.LogInformation("Successfully deleted site {SiteName} (ID: {SiteId}) from database", persisted.Name, id);

            // Update UI collection on UI thread
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var index = -1;
                for (var i = 0; i < Sites.Count; i++)
                {
                    if (Sites[i].Id == id)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0 && index < Sites.Count)
                    Sites.RemoveAt(index);

                // Choose new selection
                SiteProfile? newSelection = null;
                if (Sites.Count > 0)
                {
                    var pickIndex = Math.Min(Math.Max(0, index), Sites.Count - 1);
                    if (pickIndex >= 0 && pickIndex < Sites.Count)
                        newSelection = Sites[pickIndex];
                }

                SelectedSite = newSelection;

                if (SelectedSite == null)
                {
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

                logger?.LogDebug("UI updated after site deletion, new selection: {SelectedSite}",
                    SelectedSite?.Name ?? "(none)");
            });

            logger?.LogInformation("Site deletion completed successfully for ID {SiteId}", id);
            // Notify that the site list has changed
            OnPropertyChanged(nameof(HasNoSites));
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unexpected error during site deletion for ID {SiteId}", id);
            return false;
        }
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
    private sealed class DelegateCommand : ICommand
    {
        private readonly Action? _execute;
        private readonly Func<bool>? _canExecute;

        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

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
        logger?.LogInformation(
            "SiteManagerViewModel.OpenDocsCommand invoked, but no documentation URL is configured. " +
            "Please refer to the project README.md and solution-summary.md for current documentation.");
    });

    // Ensure CanExecute changes when selection changes
    private void RaiseCommandCanExecuteChanged()
    {
        (_deleteCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (_saveCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        (_newSiteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (_openDocsCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
