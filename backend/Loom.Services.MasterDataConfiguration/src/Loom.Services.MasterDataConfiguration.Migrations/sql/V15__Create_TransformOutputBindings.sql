-- Create TransformOutputBindings table
CREATE TABLE IF NOT EXISTS "TransformOutputBindings" (
    "Id" UUID NOT NULL,
    "TransformationSpecId" UUID NOT NULL,
    "TargetPath" VARCHAR(500) NOT NULL,
    "FromNodeId" UUID NOT NULL,
    CONSTRAINT "PK_TransformOutputBindings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TransformOutputBindings_TransformationSpecs_TransformationSpecId" 
        FOREIGN KEY ("TransformationSpecId") 
        REFERENCES "TransformationSpecs" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on TransformationSpecId and TargetPath
CREATE UNIQUE INDEX IF NOT EXISTS "IX_TransformOutputBindings_TransformationSpecId_TargetPath" 
    ON "TransformOutputBindings" ("TransformationSpecId", "TargetPath");

