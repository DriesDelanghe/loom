-- Create TransformReferences table
CREATE TABLE IF NOT EXISTS "TransformReferences" (
    "Id" UUID NOT NULL,
    "ParentTransformationSpecId" UUID NOT NULL,
    "SourceFieldPath" VARCHAR(500) NOT NULL,
    "TargetFieldPath" VARCHAR(500) NOT NULL,
    "ChildTransformationSpecId" UUID NOT NULL,
    CONSTRAINT "PK_TransformReferences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TransformReferences_TransformationSpecs_ParentTransformationSpecId" 
        FOREIGN KEY ("ParentTransformationSpecId") 
        REFERENCES "TransformationSpecs" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on ParentTransformationSpecId, SourceFieldPath, and TargetFieldPath
CREATE UNIQUE INDEX IF NOT EXISTS "IX_TransformReferences_ParentTransformationSpecId_SourceFieldPath_TargetFieldPath" 
    ON "TransformReferences" ("ParentTransformationSpecId", "SourceFieldPath", "TargetFieldPath");

