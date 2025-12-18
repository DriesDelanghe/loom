-- Create ValidationSpecs table
CREATE TABLE IF NOT EXISTS "ValidationSpecs" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "DataSchemaId" UUID NOT NULL,
    "Version" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Description" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "PublishedAt" TIMESTAMP WITH TIME ZONE,
    CONSTRAINT "PK_ValidationSpecs" PRIMARY KEY ("Id")
);

-- Create unique index on DataSchemaId and Version
CREATE UNIQUE INDEX IF NOT EXISTS "IX_ValidationSpecs_DataSchemaId_Version" 
    ON "ValidationSpecs" ("DataSchemaId", "Version");
