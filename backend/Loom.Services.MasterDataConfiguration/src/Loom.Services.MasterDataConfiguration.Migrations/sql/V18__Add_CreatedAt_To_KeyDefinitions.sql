-- Add CreatedAt column to KeyDefinitions table
ALTER TABLE "KeyDefinitions" 
ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW();

-- Update existing records to have CreatedAt set to current timestamp
UPDATE "KeyDefinitions" 
SET "CreatedAt" = NOW() 
WHERE "CreatedAt" IS NULL;


