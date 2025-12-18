-- Create WorkflowDefinitions table
CREATE TABLE IF NOT EXISTS "WorkflowDefinitions" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000),
    "IsArchived" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_WorkflowDefinitions" PRIMARY KEY ("Id")
);

-- Create index on TenantId and Name
CREATE INDEX IF NOT EXISTS "IX_WorkflowDefinitions_TenantId_Name" 
    ON "WorkflowDefinitions" ("TenantId", "Name");

