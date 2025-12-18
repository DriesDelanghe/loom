-- Create SchemaFlows table
CREATE TABLE IF NOT EXISTS "SchemaFlows" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "SourceSchemaId" UUID NOT NULL,
    "TargetSchemaId" UUID NOT NULL,
    "FlowType" INTEGER NOT NULL,
    CONSTRAINT "PK_SchemaFlows" PRIMARY KEY ("Id")
);

-- Create unique index on SourceSchemaId, TargetSchemaId, and FlowType
CREATE UNIQUE INDEX IF NOT EXISTS "IX_SchemaFlows_SourceSchemaId_TargetSchemaId_FlowType" 
    ON "SchemaFlows" ("SourceSchemaId", "TargetSchemaId", "FlowType");
