namespace FileTransferino.Core.Models;

/// <summary>
/// Represents a saved FTP/FTPS/SFTP site connection profile.
/// </summary>
public sealed class SiteProfile
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Protocol { get; set; } // "FTP", "FTPS", "SFTP"
    
    public required string Host { get; set; }
    
    public required int Port { get; set; }
    
    public string? Username { get; set; }
    
    public string? DefaultRemotePath { get; set; }
    
    public string? DefaultLocalPath { get; set; }
    
    /// <summary>
    /// Key reference to encrypted credential in secure storage.
    /// Never contains the actual password.
    /// </summary>
    public string? CredentialKey { get; set; }
    
    public DateTime CreatedUtc { get; set; }
    
    public DateTime UpdatedUtc { get; set; }
}
