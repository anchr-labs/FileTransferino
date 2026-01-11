using Dapper;
using FileTransferino.Core.Models;
using FileTransferino.Infrastructure;
using Microsoft.Data.Sqlite;

namespace FileTransferino.Data.Repositories;

/// <summary>
/// Repository implementation for site profiles using Dapper and SQLite.
/// </summary>
public sealed class SiteRepository : ISiteRepository
{
    private readonly string _connectionString;

    public SiteRepository(AppPaths paths)
    {
        var dbPath = Path.Combine(paths.Data, "FileTransferino.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public SiteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<SiteProfile>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Name, Protocol, Host, Port, Username, 
                   DefaultRemotePath, DefaultLocalPath, CredentialKey,
                   CreatedUtc, UpdatedUtc
            FROM Sites
            ORDER BY Name";

        await using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<SiteProfile>(sql);
    }

    public async Task<SiteProfile?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Name, Protocol, Host, Port, Username, 
                   DefaultRemotePath, DefaultLocalPath, CredentialKey,
                   CreatedUtc, UpdatedUtc
            FROM Sites
            WHERE Id = @Id";

        await using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<SiteProfile>(sql, new { Id = id });
    }

    public async Task<int> InsertAsync(SiteProfile site)
    {
        const string sql = @"
            INSERT INTO Sites (Name, Protocol, Host, Port, Username, 
                             DefaultRemotePath, DefaultLocalPath, CredentialKey,
                             CreatedUtc, UpdatedUtc)
            VALUES (@Name, @Protocol, @Host, @Port, @Username, 
                    @DefaultRemotePath, @DefaultLocalPath, @CredentialKey,
                    @CreatedUtc, @UpdatedUtc);
            SELECT last_insert_rowid();";

        site.CreatedUtc = DateTime.UtcNow;
        site.UpdatedUtc = DateTime.UtcNow;

        await using var connection = new SqliteConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync<int>(sql, site);
        site.Id = id;
        return id;
    }

    public async Task<bool> UpdateAsync(SiteProfile site)
    {
        const string sql = @"
            UPDATE Sites 
            SET Name = @Name,
                Protocol = @Protocol,
                Host = @Host,
                Port = @Port,
                Username = @Username,
                DefaultRemotePath = @DefaultRemotePath,
                DefaultLocalPath = @DefaultLocalPath,
                CredentialKey = @CredentialKey,
                UpdatedUtc = @UpdatedUtc
            WHERE Id = @Id";

        site.UpdatedUtc = DateTime.UtcNow;

        await using var connection = new SqliteConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, site);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Sites WHERE Id = @Id";

        await using var connection = new SqliteConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
