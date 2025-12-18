-- Change Connection.On from enum to string Outcome
-- First, add the new Outcome column
ALTER TABLE "Connections" ADD COLUMN IF NOT EXISTS "Outcome" VARCHAR(50);

-- Migrate existing data: Success -> Completed, RecoverableFailure -> Failed
-- This assumes Action nodes, which is the most common case
UPDATE "Connections" 
SET "Outcome" = CASE 
    WHEN "On" = 0 THEN 'Completed'  -- Success -> Completed
    WHEN "On" = 1 THEN 'Failed'     -- RecoverableFailure -> Failed
    ELSE 'Completed'                 -- Default fallback
END
WHERE "Outcome" IS NULL;

-- Make Outcome NOT NULL after migration
ALTER TABLE "Connections" ALTER COLUMN "Outcome" SET NOT NULL;

-- Drop the old enum column
ALTER TABLE "Connections" DROP COLUMN IF EXISTS "On";

-- Update the index to use Outcome instead of On
DROP INDEX IF EXISTS "IX_Connections_WorkflowVersionId_FromNodeId_ToNodeId_On";
CREATE INDEX IF NOT EXISTS "IX_Connections_WorkflowVersionId_FromNodeId_ToNodeId_Outcome" 
    ON "Connections" ("WorkflowVersionId", "FromNodeId", "ToNodeId", "Outcome");

