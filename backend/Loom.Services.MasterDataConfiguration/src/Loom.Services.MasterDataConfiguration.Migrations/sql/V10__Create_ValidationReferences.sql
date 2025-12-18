-- Create ValidationReferences table
CREATE TABLE IF NOT EXISTS "ValidationReferences" (
    "Id" UUID NOT NULL,
    "ParentValidationSpecId" UUID NOT NULL,
    "FieldPath" VARCHAR(500) NOT NULL,
    "ChildValidationSpecId" UUID NOT NULL,
    CONSTRAINT "PK_ValidationReferences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ValidationReferences_ValidationSpecs_ParentValidationSpecId" 
        FOREIGN KEY ("ParentValidationSpecId") 
        REFERENCES "ValidationSpecs" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on ParentValidationSpecId, FieldPath, and ChildValidationSpecId
CREATE UNIQUE INDEX IF NOT EXISTS "IX_ValidationReferences_ParentValidationSpecId_FieldPath_ChildValidationSpecId" 
    ON "ValidationReferences" ("ParentValidationSpecId", "FieldPath", "ChildValidationSpecId");
