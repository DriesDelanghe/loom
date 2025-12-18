-- Create SimpleTransformRules table
CREATE TABLE IF NOT EXISTS "SimpleTransformRules" (
    "Id" UUID NOT NULL,
    "TransformationSpecId" UUID NOT NULL,
    "SourcePath" VARCHAR(500) NOT NULL,
    "TargetPath" VARCHAR(500) NOT NULL,
    "ConverterId" UUID,
    "Required" BOOLEAN NOT NULL DEFAULT FALSE,
    "Order" INTEGER NOT NULL,
    CONSTRAINT "PK_SimpleTransformRules" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SimpleTransformRules_TransformationSpecs_TransformationSpecId" 
        FOREIGN KEY ("TransformationSpecId") 
        REFERENCES "TransformationSpecs" ("Id") 
        ON DELETE CASCADE
);

