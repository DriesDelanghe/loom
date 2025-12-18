-- Create DataModels table
CREATE TABLE IF NOT EXISTS "DataModels" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Key" VARCHAR(200) NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_DataModels" PRIMARY KEY ("Id")
);

-- Create unique index on TenantId and Key
CREATE UNIQUE INDEX IF NOT EXISTS "IX_DataModels_TenantId_Key" 
    ON "DataModels" ("TenantId", "Key");
