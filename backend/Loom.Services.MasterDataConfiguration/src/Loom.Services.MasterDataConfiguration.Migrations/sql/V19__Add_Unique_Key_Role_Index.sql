-- Add unique index on TenantId, Key, and Role
-- This ensures schemas are unique by key and role combination
CREATE UNIQUE INDEX IF NOT EXISTS "IX_DataSchemas_TenantId_Key_Role" 
    ON "DataSchemas" ("TenantId", "Key", "Role");

