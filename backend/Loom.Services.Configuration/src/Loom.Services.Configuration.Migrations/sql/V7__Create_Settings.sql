-- Create Settings table
CREATE TABLE IF NOT EXISTS "Settings" (
    "Id" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "MaxNodeExecutions" INTEGER,
    "MaxDurationSeconds" INTEGER,
    CONSTRAINT "PK_Settings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Settings_WorkflowVersions_WorkflowVersionId" 
        FOREIGN KEY ("WorkflowVersionId") 
        REFERENCES "WorkflowVersions" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on WorkflowVersionId
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Settings_WorkflowVersionId" 
    ON "Settings" ("WorkflowVersionId");

