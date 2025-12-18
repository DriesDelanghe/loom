-- Create TriggerBindings table
CREATE TABLE IF NOT EXISTS "TriggerBindings" (
    "Id" UUID NOT NULL,
    "TriggerId" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "Enabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "Priority" INTEGER,
    CONSTRAINT "PK_TriggerBindings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TriggerBindings_Triggers_TriggerId" 
        FOREIGN KEY ("TriggerId") 
        REFERENCES "Triggers" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_TriggerBindings_WorkflowVersions_WorkflowVersionId" 
        FOREIGN KEY ("WorkflowVersionId") 
        REFERENCES "WorkflowVersions" ("Id") 
        ON DELETE CASCADE
);

-- Create index on TriggerId and WorkflowVersionId
CREATE INDEX IF NOT EXISTS "IX_TriggerBindings_TriggerId_WorkflowVersionId" 
    ON "TriggerBindings" ("TriggerId", "WorkflowVersionId");

