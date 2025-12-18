-- Create Variables table
CREATE TABLE IF NOT EXISTS "Variables" (
    "Id" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "Key" VARCHAR(200) NOT NULL,
    "Type" INTEGER NOT NULL,
    "InitialValueJson" TEXT,
    "Description" TEXT,
    CONSTRAINT "PK_Variables" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Variables_WorkflowVersions_WorkflowVersionId" 
        FOREIGN KEY ("WorkflowVersionId") 
        REFERENCES "WorkflowVersions" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on WorkflowVersionId and Key
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Variables_WorkflowVersionId_Key" 
    ON "Variables" ("WorkflowVersionId", "Key");

