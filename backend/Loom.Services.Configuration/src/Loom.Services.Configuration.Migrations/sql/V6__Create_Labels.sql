-- Create Labels table
CREATE TABLE IF NOT EXISTS "Labels" (
    "Id" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "Key" VARCHAR(200) NOT NULL,
    "Type" INTEGER NOT NULL,
    "Description" TEXT,
    CONSTRAINT "PK_Labels" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Labels_WorkflowVersions_WorkflowVersionId" 
        FOREIGN KEY ("WorkflowVersionId") 
        REFERENCES "WorkflowVersions" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on WorkflowVersionId and Key
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Labels_WorkflowVersionId_Key" 
    ON "Labels" ("WorkflowVersionId", "Key");

