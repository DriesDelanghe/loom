-- Create TransformationSpecs table
CREATE TABLE IF NOT EXISTS "TransformationSpecs" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "SourceSchemaId" UUID NOT NULL,
    "TargetSchemaId" UUID NOT NULL,
    "Mode" INTEGER NOT NULL,
    "Cardinality" INTEGER NOT NULL,
    "Version" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Description" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "PublishedAt" TIMESTAMP WITH TIME ZONE,
    CONSTRAINT "PK_TransformationSpecs" PRIMARY KEY ("Id")
);

-- Create unique index on SourceSchemaId, TargetSchemaId, and Version
CREATE UNIQUE INDEX IF NOT EXISTS "IX_TransformationSpecs_SourceSchemaId_TargetSchemaId_Version" 
    ON "TransformationSpecs" ("SourceSchemaId", "TargetSchemaId", "Version");
