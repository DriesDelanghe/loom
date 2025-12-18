-- Create ValidationRules table
CREATE TABLE IF NOT EXISTS "ValidationRules" (
    "Id" UUID NOT NULL,
    "ValidationSpecId" UUID NOT NULL,
    "RuleType" INTEGER NOT NULL,
    "Severity" INTEGER NOT NULL,
    "Parameters" JSONB NOT NULL,
    CONSTRAINT "PK_ValidationRules" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ValidationRules_ValidationSpecs_ValidationSpecId" 
        FOREIGN KEY ("ValidationSpecId") 
        REFERENCES "ValidationSpecs" ("Id") 
        ON DELETE CASCADE
);
