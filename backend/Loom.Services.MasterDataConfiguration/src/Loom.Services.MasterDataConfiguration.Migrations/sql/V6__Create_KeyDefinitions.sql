-- Create KeyDefinitions table
CREATE TABLE IF NOT EXISTS "KeyDefinitions" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "DataSchemaId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "IsPrimary" BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_KeyDefinitions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_KeyDefinitions_DataSchemas_DataSchemaId" 
        FOREIGN KEY ("DataSchemaId") 
        REFERENCES "DataSchemas" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on DataSchemaId and Name
CREATE UNIQUE INDEX IF NOT EXISTS "IX_KeyDefinitions_DataSchemaId_Name" 
    ON "KeyDefinitions" ("DataSchemaId", "Name");
