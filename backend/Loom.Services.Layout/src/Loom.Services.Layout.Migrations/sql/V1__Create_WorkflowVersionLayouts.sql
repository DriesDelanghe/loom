CREATE TABLE "WorkflowVersionLayouts" (
    "TenantId" UUID NOT NULL,
    "WorkflowVersionId" UUID NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "PK_WorkflowVersionLayouts" PRIMARY KEY ("TenantId", "WorkflowVersionId")
);

CREATE INDEX "IX_WorkflowVersionLayouts_WorkflowVersionId" ON "WorkflowVersionLayouts" ("WorkflowVersionId");

