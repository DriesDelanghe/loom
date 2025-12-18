-- Create FieldDefinitions table
CREATE TABLE IF NOT EXISTS "FieldDefinitions" (
    "Id" UUID NOT NULL,
    "DataSchemaId" UUID NOT NULL,
    "Path" VARCHAR(500) NOT NULL,
    "FieldType" INTEGER NOT NULL,
    "ScalarType" INTEGER,
    "ElementSchemaId" UUID,
    "Required" BOOLEAN NOT NULL DEFAULT FALSE,
    "Description" VARCHAR(2000),
    CONSTRAINT "PK_FieldDefinitions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FieldDefinitions_DataSchemas_DataSchemaId" 
        FOREIGN KEY ("DataSchemaId") 
        REFERENCES "DataSchemas" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on DataSchemaId and Path
CREATE UNIQUE INDEX IF NOT EXISTS "IX_FieldDefinitions_DataSchemaId_Path" 
    ON "FieldDefinitions" ("DataSchemaId", "Path");
