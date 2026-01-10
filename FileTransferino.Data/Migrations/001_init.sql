-- Initial migration to verify DbUp is working correctly
-- Creates a simple schema info table

CREATE TABLE IF NOT EXISTS __SchemaInfo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CreatedUtc TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Insert a record to mark initial setup
INSERT INTO __SchemaInfo (CreatedUtc) VALUES (datetime('now'));
