-- Create KeyFields table
CREATE TABLE IF NOT EXISTS "KeyFields" (
    "Id" UUID NOT NULL,
    "KeyDefinitionId" UUID NOT NULL,
    "FieldPath" VARCHAR(500) NOT NULL,
    "Order" INTEGER NOT NULL,
    "Normalization" VARCHAR(100),
    CONSTRAINT "PK_KeyFields" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_KeyFields_KeyDefinitions_KeyDefinitionId" 
        FOREIGN KEY ("KeyDefinitionId") 
        REFERENCES "KeyDefinitions" ("Id") 
        ON DELETE CASCADE
);

-- Create unique index on KeyDefinitionId and Order
CREATE UNIQUE INDEX IF NOT EXISTS "IX_KeyFields_KeyDefinitionId_Order" 
    ON "KeyFields" ("KeyDefinitionId", "Order");
