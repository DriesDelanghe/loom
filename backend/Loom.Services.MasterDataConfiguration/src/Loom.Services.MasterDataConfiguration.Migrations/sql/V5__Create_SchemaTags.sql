-- Create SchemaTags table
CREATE TABLE IF NOT EXISTS "SchemaTags" (
    "Id" UUID NOT NULL,
    "DataSchemaId" UUID NOT NULL,
    "Tag" VARCHAR(100) NOT NULL,
    CONSTRAINT "PK_SchemaTags" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SchemaTags_DataSchemas_DataSchemaId" 
        FOREIGN KEY ("DataSchemaId") 
        REFERENCES "DataSchemas" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on DataSchemaId and Tag
CREATE UNIQUE INDEX IF NOT EXISTS "IX_SchemaTags_DataSchemaId_Tag" 
    ON "SchemaTags" ("DataSchemaId", "Tag");
