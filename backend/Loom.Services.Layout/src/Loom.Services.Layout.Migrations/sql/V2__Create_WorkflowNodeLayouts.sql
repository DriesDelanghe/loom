CREATE TABLE "WorkflowNodeLayouts" (
    "TenantId" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "NodeKey" VARCHAR(200) NOT NULL,
    "X" DECIMAL(18,2) NOT NULL,
    "Y" DECIMAL(18,2) NOT NULL,
    "Width" DECIMAL(18,2) NULL,
    "Height" DECIMAL(18,2) NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "PK_WorkflowNodeLayouts" PRIMARY KEY ("TenantId", "WorkflowVersionId", "NodeKey")
);

CREATE INDEX "IX_WorkflowNodeLayouts_WorkflowVersionId_NodeKey" ON "WorkflowNodeLayouts" ("WorkflowVersionId", "NodeKey");

