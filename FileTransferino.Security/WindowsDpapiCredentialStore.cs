using System.Security.Cryptography;
using System.Text;
using FileTransferino.Infrastructure;

namespace FileTransferino.Security;

/// <summary>
/// Credential store implementation using Windows DPAPI for encryption.
/// Stores encrypted credentials as files in {AppPaths.Root}/secrets/
/// </summary>
public sealed class WindowsDpapiCredentialStore : ICredentialStore
{
    private readonly string _secretsPath;

    public WindowsDpapiCredentialStore(AppPaths paths)
    {
        _secretsPath = Path.Combine(paths.Root, "secrets");
        EnsureSecretsDirectoryExists();
    }

    private void EnsureSecretsDirectoryExists()
    {
        if (!Directory.Exists(_secretsPath))
        {
            Directory.CreateDirectory(_secretsPath);
            
            // Set directory attributes to hidden for security
            try
            {
                var dirInfo = new DirectoryInfo(_secretsPath);
                dirInfo.Attributes |= FileAttributes.Hidden;
            }
            catch
            {
                // Ignore if we can't set hidden attribute
            }
        }
    }

    private string GetFilePath(string key)
    {
        // Sanitize key to be filename-safe
        var safeKey = string.Concat(key.Select(c => 
            char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_'));
        
        return Path.Combine(_secretsPath, $"{safeKey}.dat");
    }

    public async Task SaveAsync(string key, string secret)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));

        EnsureSecretsDirectoryExists();

        // Convert secret to bytes
        var secretBytes = Encoding.UTF8.GetBytes(secret);

        // Encrypt using DPAPI (user scope)
        var encryptedBytes = ProtectedData.Protect(
            secretBytes,
            optionalEntropy: null,
            scope: DataProtectionScope.CurrentUser);

        // Write encrypted bytes to file
        var filePath = GetFilePath(key);
        await File.WriteAllBytesAsync(filePath, encryptedBytes);
    }

    public async Task<string?> GetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var filePath = GetFilePath(key);
        
        if (!File.Exists(filePath))
            return null;

        try
        {
            // Read encrypted bytes
            var encryptedBytes = await File.ReadAllBytesAsync(filePath);

            // Decrypt using DPAPI
            var secretBytes = ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            // Convert back to string
            return Encoding.UTF8.GetString(secretBytes);
        }
        catch (CryptographicException)
        {
            // Decryption failed (e.g., encrypted by different user)
            return null;
        }
        catch
        {
            // Other errors (file corrupted, etc.)
            return null;
        }
    }

    public Task<bool> DeleteAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult(false);

        var filePath = GetFilePath(key);
        
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        try
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
