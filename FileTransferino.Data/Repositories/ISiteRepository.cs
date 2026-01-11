using FileTransferino.Core.Models;

namespace FileTransferino.Data.Repositories;

/// <summary>
/// Repository for managing site profiles in the database.
/// </summary>
public interface ISiteRepository
{
    Task<IEnumerable<SiteProfile>> GetAllAsync();
    
    Task<SiteProfile?> GetByIdAsync(int id);
    
    Task<int> InsertAsync(SiteProfile site);
    
    Task<bool> UpdateAsync(SiteProfile site);
    
    Task<bool> DeleteAsync(int id);
}
