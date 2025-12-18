# Functional Workflows

This document describes the key functional workflows and behaviors in the Configuration Service.

## Workflow Lifecycle

### Creating a New Workflow

1. **Create Workflow Definition**
   - Provide tenant ID, name, and optional description
   - System creates a new workflow definition with no versions
   - Workflow is in Active state

2. **Create Initial Draft Version**
   - Select the workflow definition
   - System creates a new draft version (version 1)
   - Version starts empty (no nodes, connections, triggers)

3. **Configure Workflow**
   - Add nodes (Action, Condition, Validation, Split, Join)
   - Connect nodes (outcomes are determined by connector position in UI)
   - Configure triggers and bind them to entry nodes
   - Add variables and labels
   - Configure workflow settings

4. **Validate Workflow**
   - System validates:
     - All nodes have valid configurations
     - Graph is connected (no orphaned nodes)
     - All entry nodes are reachable from triggers
     - Required trigger bindings exist
   - Validation returns errors and warnings

5. **Publish Workflow**
   - System validates the workflow meets all publishing requirements
   - If valid, version status changes from Draft to Published
   - Version becomes immutable (read-only)
   - Workflow can now be executed by the workflow engine

### Creating a New Draft from Published Version

1. **Select Published Version**
   - Choose a published version to base the new draft on

2. **Create Draft Version**
   - System creates a new draft version with incremented version number
   - All nodes are copied with new IDs but same keys
   - All connections are copied and mapped to new node IDs
   - All trigger bindings are copied
   - All variables, labels, and settings are copied

3. **Modify Draft**
   - Edit nodes, connections, triggers as needed
   - Changes only affect the draft version
   - Published version remains unchanged

4. **Publish New Version**
   - Validate and publish the new draft
   - New version becomes Published
   - Previous published version remains Published (multiple published versions can exist)

### Archiving Workflows

1. **Archive Workflow Definition**
   - Archive a workflow definition
   - All versions remain accessible but workflow is hidden from normal operations
   - Can be unarchived if needed

2. **Archive Workflow Version**
   - Archive a specific version
   - Version is preserved but no longer active
   - Cannot be executed or used as source for new drafts

### Deleting Draft Versions

- Only Draft versions can be deleted
- Published versions cannot be deleted (preserved for audit/history)
- Deleting a draft removes all associated nodes, connections, triggers, variables, and labels

## Node Management

### Adding a Node

1. **Specify Node Details**
   - Provide workflow version ID
   - Provide unique key (string identifier)
   - Provide optional name
   - Select node type (Action, Condition, Validation, Split, Join)
   - Provide type-specific configuration (JSON)
     - Split nodes: `{ "maxParallelism": number }`
     - Join nodes: `{ "joinType": "All" | "Any", "cancelRemaining": boolean }`

2. **System Creates Node**
   - Node is added to the workflow version
   - Node can only be added to Draft versions
   - Key must be unique within the workflow version
   - Node category is automatically determined from node type

### Updating Node Metadata

- **Name**: Can be updated in Draft versions
- **Type**: Can be changed in Draft versions (may require config updates)
- **Config**: Can be updated in Draft versions
- Published versions: All node properties are read-only

### Removing a Node

- Node can only be removed from Draft versions
- Removing a node also removes all connections to/from that node
- If the node is an entry point for a trigger, the trigger node binding must be removed first

## Connection Management

### Adding a Connection

1. **Create Connection in UI**
   - Drag from source node connector to target node
   - Outcome is automatically determined by connector position:
     - Top connector = positive outcome (Completed/True/Valid)
     - Bottom connector = negative outcome (Failed/False/Invalid)
     - Control nodes use fixed outcomes (Split: Next, Join: Joined)

2. **System Validates**
   - Both nodes must exist in the workflow version
   - Outcome must be valid for source node's category
   - Duplicate connections (same from, to, outcome) are not allowed
   - Connection can only be added to Draft versions
   - Required outcomes must be present (all outcomes for non-Control nodes)

3. **System Creates Connection**
   - Connection is added to the workflow graph with semantic outcome
   - Connection ID is returned

### Position-Based Connection Outcomes

In the UI, connection outcomes are determined by the connector position on the source node:
- **Top connector**: Positive outcome
  - Action → "Completed"
  - Condition → "True"
  - Validation → "Valid"
- **Bottom connector**: Negative outcome
  - Action → "Failed"
  - Condition → "False"
  - Validation → "Invalid"
- **Control nodes**: Fixed outcomes
  - Split → "Next" (all outgoing connections)
  - Join → "Joined" (single outgoing connection)

Users cannot manually select outcomes; they are inferred from connector position.

### Removing a Connection

- Connection can only be removed from Draft versions
- Removing a connection may create orphaned nodes (validation will warn)

## Trigger Management

### Creating a Trigger

1. **Specify Trigger Details**
   - Provide tenant ID
   - Select trigger type (Manual, Webhook, Schedule)
   - Provide type-specific configuration

2. **System Creates Trigger**
   - Trigger is tenant-scoped and can be reused across workflow versions
   - Trigger ID is returned

### Binding Trigger to Workflow Version

1. **Create Trigger Binding**
   - Provide trigger ID and workflow version ID
   - Specify enabled status
   - Optional: Specify priority

2. **System Creates Binding**
   - Binding makes the trigger available for the workflow version
   - Binding can only be created for Draft versions
   - Multiple triggers can be bound to the same workflow version

### Binding Trigger to Entry Node

1. **Create Trigger Node Binding**
   - Provide trigger binding ID and entry node ID
   - Optional: Specify order (for multiple entry nodes)

2. **System Creates Node Binding**
   - Entry node is explicitly marked as a start point for the trigger
   - Node must exist in the workflow version
   - Multiple nodes can be entry points for the same trigger

### Unbinding Trigger

- **Unbind from Node**: Remove a specific entry node binding
- **Unbind from Workflow**: Remove the entire trigger binding (removes all node bindings)

## Variable and Label Management

### Adding Variables/Labels

1. **Specify Details**
   - Provide workflow version ID
   - Provide unique key
   - Select type (String, Number, Boolean, Object, Array)
   - Optional: Provide initial value (for variables)
   - Optional: Provide description

2. **System Creates Variable/Label**
   - Key must be unique within the workflow version
   - Can only be added to Draft versions

### Updating Variables

- Variable type, initial value, and description can be updated in Draft versions
- Published versions: Variables are read-only

### Removing Variables/Labels

- Can only be removed from Draft versions
- Removing a variable/label may break node configurations that reference it (validation will warn)

## Validation

### Validation Process

When validating a workflow version, the system checks:

1. **Graph Structure**
   - All nodes are reachable from at least one entry point
   - No cycles that would cause infinite loops
   - All connections reference valid nodes
   - At least one end node exists (node with no outgoing connections)
   - End nodes are not Control nodes
   - End nodes are not trigger entry nodes

2. **Node Configuration**
   - Each node has valid configuration for its type
   - Required configuration fields are present
   - Configuration values are of correct types
   - Split nodes: maxParallelism is a positive integer
   - Join nodes: joinType is "All" or "Any", cancelRemaining is boolean

3. **Connection Outcomes**
   - All outcomes are valid for their source node's category
   - Required outcomes are present for each non-Control node:
     - Action: Both "Completed" and "Failed"
     - Condition: Both "True" and "False"
     - Validation: Both "Valid" and "Invalid"
   - No duplicate semantic outcomes per node (for non-Control nodes)
   - Control node structural constraints:
     - Split: Exactly 1 incoming, at least 2 outgoing, all use "Next"
     - Join: At least 2 incoming, exactly 1 outgoing, uses "Joined"

4. **Trigger Configuration**
   - At least one trigger binding exists
   - Each trigger binding has at least one entry node
   - All entry nodes exist in the workflow version

5. **Variable/Label References**
   - Variables/labels referenced in node configurations exist
   - Variable types match expected types

6. **Publishing Requirements**
   - All validation checks pass
   - Workflow is ready for execution

### Validation Results

- **Is Valid**: Boolean indicating if workflow can be published
- **Errors**: Blocking issues that prevent publishing
- **Warnings**: Non-blocking issues that should be reviewed

## Compilation

### Compilation Process

When compiling a published workflow version:

1. **Load Workflow Data**
   - Load all nodes, connections, variables, labels, settings
   - Load all trigger bindings and their node bindings

2. **Build Execution Graph**
   - Create node graph with connections
   - Map triggers to entry nodes
   - Include all configurations

3. **Generate Compiled Output**
   - Structured format ready for workflow engine
   - Includes all execution metadata
   - Optimized for runtime performance

### Compiled Workflow Structure

- **Version**: Workflow version information
- **Nodes**: All nodes with configurations and categories
- **Connections**: All connections with semantic outcomes
- **Variables**: All variable definitions
- **Labels**: All label definitions
- **Settings**: Execution settings
- **Triggers**: All triggers with explicit entry node mappings

The compiled workflow is what the workflow engine consumes to execute workflows. It includes explicit trigger-to-entry-node mappings, removing the need for implicit inference.

