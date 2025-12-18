-- Remove the incorrect unique constraint on (TenantId, Key, Version)
-- The unique constraint should only be on (TenantId, Key, Role) to allow
-- the same key across different roles (Incoming, Master, Outgoing)
DROP INDEX IF EXISTS "IX_DataSchemas_TenantId_Key_Version";

-- Keep a non-unique index on (TenantId, Key, Version) for query performance
-- This allows efficient lookups of specific schema versions
CREATE INDEX IF NOT EXISTS "IX_DataSchemas_TenantId_Key_Version" 
    ON "DataSchemas" ("TenantId", "Key", "Version");

