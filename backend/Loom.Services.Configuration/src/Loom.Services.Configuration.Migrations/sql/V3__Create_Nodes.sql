-- Create Nodes table
CREATE TABLE IF NOT EXISTS "Nodes" (
    "Id" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "Key" VARCHAR(200) NOT NULL,
    "Name" VARCHAR(200),
    "Type" INTEGER NOT NULL,
    "ConfigJson" TEXT,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Nodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Nodes_WorkflowVersions_WorkflowVersionId" 
        FOREIGN KEY ("WorkflowVersionId") 
        REFERENCES "WorkflowVersions" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on WorkflowVersionId and Key
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Nodes_WorkflowVersionId_Key" 
    ON "Nodes" ("WorkflowVersionId", "Key");

