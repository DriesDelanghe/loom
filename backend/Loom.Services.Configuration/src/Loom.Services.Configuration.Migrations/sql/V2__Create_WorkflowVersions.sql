-- Create WorkflowVersions table
CREATE TABLE IF NOT EXISTS "WorkflowVersions" (
    "Id" UUID NOT NULL,
    "DefinitionId" UUID NOT NULL,
    "Version" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedBy" VARCHAR(200),
    "PublishedAt" TIMESTAMP WITH TIME ZONE,
    "PublishedBy" VARCHAR(200),
    CONSTRAINT "PK_WorkflowVersions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_WorkflowVersions_WorkflowDefinitions_DefinitionId" 
        FOREIGN KEY ("DefinitionId") 
        REFERENCES "WorkflowDefinitions" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on DefinitionId and Version
CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkflowVersions_DefinitionId_Version" 
    ON "WorkflowVersions" ("DefinitionId", "Version");

