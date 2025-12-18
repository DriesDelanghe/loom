-- Create DataSchemas table
CREATE TABLE IF NOT EXISTS "DataSchemas" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "DataModelId" UUID,
    "Role" INTEGER NOT NULL DEFAULT 0,
    "Key" VARCHAR(200) NOT NULL,
    "Version" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Description" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "PublishedAt" TIMESTAMP WITH TIME ZONE,
    CONSTRAINT "PK_DataSchemas" PRIMARY KEY ("Id")
);

-- Create unique index on TenantId, Key, and Version
CREATE UNIQUE INDEX IF NOT EXISTS "IX_DataSchemas_TenantId_Key_Version" 
    ON "DataSchemas" ("TenantId", "Key", "Version");
