-- Create TransformGraphNodes table
CREATE TABLE IF NOT EXISTS "TransformGraphNodes" (
    "Id" UUID NOT NULL,
    "TransformationSpecId" UUID NOT NULL,
    "Key" VARCHAR(200) NOT NULL,
    "NodeType" INTEGER NOT NULL,
    "OutputType" JSONB NOT NULL,
    "Config" JSONB NOT NULL,
    CONSTRAINT "PK_TransformGraphNodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TransformGraphNodes_TransformationSpecs_TransformationSpecId" 
        FOREIGN KEY ("TransformationSpecId") 
        REFERENCES "TransformationSpecs" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on TransformationSpecId and Key
CREATE UNIQUE INDEX IF NOT EXISTS "IX_TransformGraphNodes_TransformationSpecId_Key" 
    ON "TransformGraphNodes" ("TransformationSpecId", "Key");


