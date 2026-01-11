-- Migration 002: Create Sites table for FTP/FTPS/SFTP site profiles

CREATE TABLE Sites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Protocol TEXT NOT NULL,
    Host TEXT NOT NULL,
    Port INTEGER NOT NULL,
    Username TEXT NULL,
    DefaultRemotePath TEXT NULL,
    DefaultLocalPath TEXT NULL,
    CredentialKey TEXT NULL,
    CreatedUtc INTEGER NOT NULL DEFAULT (strftime('%s','now')),
    UpdatedUtc INTEGER NOT NULL DEFAULT (strftime('%s','now'))
);

-- Index for efficient lookup by connection details
CREATE INDEX IF NOT EXISTS IX_Sites_Host_Port_Username 
ON Sites(Host, Port, Username);
