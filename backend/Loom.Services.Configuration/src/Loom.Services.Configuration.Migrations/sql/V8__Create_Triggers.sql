-- Create Triggers table
CREATE TABLE IF NOT EXISTS "Triggers" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Type" INTEGER NOT NULL,
    "ConfigJson" TEXT,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Triggers" PRIMARY KEY ("Id")
);

-- Create index on TenantId
CREATE INDEX IF NOT EXISTS "IX_Triggers_TenantId" 
    ON "Triggers" ("TenantId");

