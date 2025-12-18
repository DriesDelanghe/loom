-- Create TransformGraphEdges table
CREATE TABLE IF NOT EXISTS "TransformGraphEdges" (
    "Id" UUID NOT NULL,
    "TransformationSpecId" UUID NOT NULL,
    "FromNodeId" UUID NOT NULL,
    "ToNodeId" UUID NOT NULL,
    "InputName" VARCHAR(200) NOT NULL,
    "Order" INTEGER NOT NULL,
    CONSTRAINT "PK_TransformGraphEdges" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TransformGraphEdges_TransformationSpecs_TransformationSpecId" 
        FOREIGN KEY ("TransformationSpecId") 
        REFERENCES "TransformationSpecs" ("Id") 
        ON DELETE CASCADE
);


