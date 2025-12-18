CREATE TABLE "TriggerNodeBindings" (
    "Id" UUID PRIMARY KEY,
    "TriggerBindingId" UUID NOT NULL,
    "EntryNodeId" UUID NOT NULL,
    "Order" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_TriggerNodeBindings_TriggerBindings" FOREIGN KEY ("TriggerBindingId") REFERENCES "TriggerBindings" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TriggerNodeBindings_Nodes" FOREIGN KEY ("EntryNodeId") REFERENCES "Nodes" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_TriggerNodeBindings_TriggerBindingId_EntryNodeId" ON "TriggerNodeBindings" ("TriggerBindingId", "EntryNodeId");
CREATE INDEX "IX_TriggerNodeBindings_EntryNodeId" ON "TriggerNodeBindings" ("EntryNodeId");

