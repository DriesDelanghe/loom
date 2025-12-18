-- Create Connections table
CREATE TABLE IF NOT EXISTS "Connections" (
    "Id" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "FromNodeId" UUID NOT NULL,
    "ToNodeId" UUID NOT NULL,
    "On" INTEGER NOT NULL,
    "Order" INTEGER,
    CONSTRAINT "PK_Connections" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Connections_WorkflowVersions_WorkflowVersionId" 
        FOREIGN KEY ("WorkflowVersionId") 
        REFERENCES "WorkflowVersions" ("Id") 
        ON DELETE CASCADE
);

-- Create index on WorkflowVersionId, FromNodeId, ToNodeId, and On
CREATE INDEX IF NOT EXISTS "IX_Connections_WorkflowVersionId_FromNodeId_ToNodeId_On" 
    ON "Connections" ("WorkflowVersionId", "FromNodeId", "ToNodeId", "On");

