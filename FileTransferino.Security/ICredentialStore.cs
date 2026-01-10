namespace FileTransferino.Security;

/// <summary>
/// Secure storage for sensitive credentials (passwords, keys, etc).
/// Implementations must encrypt data at rest.
/// </summary>
public interface ICredentialStore
{
    /// <summary>
    /// Saves a secret securely using the specified key.
    /// </summary>
    /// <param name="key">Unique identifier for the secret.</param>
    /// <param name="secret">The secret to store (will be encrypted).</param>
    Task SaveAsync(string key, string secret);

    /// <summary>
    /// Retrieves a secret by key.
    /// </summary>
    /// <param name="key">Unique identifier for the secret.</param>
    /// <returns>The decrypted secret, or null if not found.</returns>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Deletes a secret by key.
    /// </summary>
    /// <param name="key">Unique identifier for the secret.</param>
    Task<bool> DeleteAsync(string key);
}
