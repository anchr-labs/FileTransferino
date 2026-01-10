using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileTransferino.Core.Models;
using FileTransferino.Data.Repositories;
using FileTransferino.Infrastructure;
using FileTransferino.Security;

namespace FileTransferino.App.ViewModels;

/// <summary>
/// ViewModel for the Site Manager window.
/// </summary>
public sealed class SiteManagerViewModel : INotifyPropertyChanged
{
    private readonly ISiteRepository _siteRepository;
    private readonly ICredentialStore _credentialStore;
    
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

    public SiteManagerViewModel(ISiteRepository siteRepository, ICredentialStore credentialStore)
    {
        _siteRepository = siteRepository;
        _credentialStore = credentialStore;
    }

    public SiteProfile? SelectedSite
    {
        get => _selectedSite;
        set
        {
            if (_selectedSite != value)
            {
                _selectedSite = value;
                OnPropertyChanged();
                LoadSelectedSite();
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public string Protocol
    {
        get => _protocol;
        set
        {
            if (_protocol != value)
            {
                _protocol = value;
                OnPropertyChanged();
                UpdateDefaultPort();
            }
        }
    }

    public string Host
    {
        get => _host;
        set
        {
            if (_host != value)
            {
                _host = value;
                OnPropertyChanged();
            }
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            if (_port != value)
            {
                _port = value;
                OnPropertyChanged();
            }
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                OnPropertyChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                _isPasswordChanged = !string.IsNullOrEmpty(value);
                OnPropertyChanged();
            }
        }
    }

    public string DefaultRemotePath
    {
        get => _defaultRemotePath;
        set
        {
            if (_defaultRemotePath != value)
            {
                _defaultRemotePath = value;
                OnPropertyChanged();
            }
        }
    }

    public string DefaultLocalPath
    {
        get => _defaultLocalPath;
        set
        {
            if (_defaultLocalPath != value)
            {
                _defaultLocalPath = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task LoadSitesAsync()
    {
        Sites.Clear();
        var sites = await _siteRepository.GetAllAsync();
        foreach (var site in sites)
        {
            Sites.Add(site);
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
            return false;

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
                await _credentialStore.SaveAsync(site.CredentialKey, Password);
            }

            if (isNew)
            {
                await _siteRepository.InsertAsync(site);
                Sites.Add(site);
                SelectedSite = site;
            }
            else
            {
                await _siteRepository.UpdateAsync(site);
                
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
            }

            Password = string.Empty;
            _isPasswordChanged = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Safely delete a site by id. Re-fetches the persisted record to obtain credential key
    /// and performs DB + credential deletion. Updates the UI collection on the UI thread.
    /// </summary>
    public async Task<bool> DeleteSiteByIdAsync(int id)
    {
        try
        {
            // Re-fetch persisted site to get up-to-date credential key
            var persisted = await _siteRepository.GetByIdAsync(id);
            if (persisted == null)
            {
                return false;
            }

            // Delete credential if present
            if (!string.IsNullOrEmpty(persisted.CredentialKey))
            {
                try
                {
                    await _credentialStore.DeleteAsync(persisted.CredentialKey);
                }
                catch
                {
                    // Credential deletion failed, but continue with site deletion
                }
            }

            // Delete from DB
            var deleted = await _siteRepository.DeleteAsync(id);
            if (!deleted)
            {
                return false;
            }

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
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Backwards-compatible: delete using selected site reference
    public Task<bool> DeleteSiteAsync() => DeleteSiteByIdAsync(SelectedSite?.Id ?? 0);

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
