# Core Concepts

This document explains the fundamental concepts and entities managed by the Configuration Service.

## Workflow Definition

A **Workflow Definition** is the top-level container for a workflow. It represents a named business process that can have multiple versions.

### Properties

- **ID**: Unique identifier (UUID)
- **Tenant ID**: Multi-tenant isolation
- **Name**: Human-readable name (required, max 200 chars)
- **Description**: Optional description (max 2000 chars)
- **Archived**: Boolean flag indicating if the workflow is archived

### Lifecycle

- **Active**: Default state, workflow can be edited and published
- **Archived**: Workflow is hidden from normal operations but preserved for historical purposes

## Workflow Version

A **Workflow Version** represents a specific configuration state of a workflow. Each workflow definition can have multiple versions, allowing for versioning and change management.

### Version States

1. **Draft**: Editable version that can be modified
   - All nodes, connections, triggers can be added/removed/modified
   - Cannot be executed by the workflow engine
   - Can be published to become a Published version

2. **Published**: Immutable, executable version
   - Cannot be modified (read-only)
   - Can be executed by the workflow engine
   - Used as the source when creating new draft versions

3. **Archived**: Historical version that is no longer active
   - Preserved for audit/historical purposes
   - Cannot be modified or executed

### Version Numbering

- Versions are auto-incremented integers (1, 2, 3, ...)
- Each workflow definition maintains its own version sequence
- Version numbers are unique within a workflow definition

### Copy-Forward Behavior

When creating a new draft version from a published version:
- All nodes are copied with new IDs but same keys
- All connections are copied (mapped to new node IDs)
- All trigger bindings are copied
- All variables and labels are copied
- All settings are copied
- The new version starts as Draft and can be modified

## Node

A **Node** represents a step in the workflow execution graph. Each node has a type that determines its behavior during execution.

### Node Categories

Nodes are organized into categories that define their semantic behavior:

1. **Action**: Performs an action (e.g., HTTP call, database operation)
2. **Condition**: Evaluates a condition and branches execution
3. **Validation**: Validates data or conditions
4. **Control**: Structural nodes that manage flow (Split, Join)

### Node Types

1. **Action** (Category: Action)
   - Performs an action during workflow execution
   - Examples: HTTP call, database operation, external service call

2. **Condition** (Category: Condition)
   - Evaluates a condition and branches execution based on the result
   - Examples: if/else logic, comparison checks

3. **Validation** (Category: Validation)
   - Validates data or conditions
   - Examples: data validation, schema validation

4. **Split** (Category: Control)
   - Splits execution into multiple parallel branches
   - Must have exactly 1 incoming connection
   - Must have at least 2 outgoing connections
   - All outgoing connections use the "Next" outcome

5. **Join** (Category: Control)
   - Joins multiple parallel branches back together
   - Must have at least 2 incoming connections
   - Must have exactly 1 outgoing connection
   - Outgoing connection uses the "Joined" outcome

### Node Properties

- **ID**: Unique identifier (UUID)
- **Key**: Stable identifier within a workflow version (string, max 200 chars)
- **Name**: Human-readable name (optional, max 200 chars)
- **Type**: Node type (Action, Condition, Validation, Split, Join)
- **Category**: Derived from node type (Action, Condition, Validation, Control)
- **Config**: Type-specific configuration (JSON object)

### Control Node Configurations

**Split Node Config**:
```json
{
  "maxParallelism": 3  // Maximum number of parallel branches (positive integer)
}
```

**Join Node Config**:
```json
{
  "joinType": "All",           // "All" or "Any"
  "cancelRemaining": true      // Boolean
}
```

### Node Keys

- Keys are stable identifiers that persist across version copies
- When a draft is created from a published version, nodes keep the same keys but get new IDs
- Keys must be unique within a workflow version
- Keys are used by the Layout Service to maintain node positions across versions

## Connection

A **Connection** represents a directed edge in the workflow graph, connecting one node to another. Connections use semantic outcomes to indicate the execution path.

### Connection Properties

- **ID**: Unique identifier (UUID)
- **From Node ID**: Source node
- **To Node ID**: Target node
- **Outcome**: Semantic outcome string (e.g., "Completed", "Failed", "True", "False", "Valid", "Invalid", "Next", "Joined")
- **Order**: Optional ordering for multiple connections from the same node

### Semantic Outcomes

Outcomes are semantic strings that indicate the execution path. The valid outcomes depend on the source node's category:

**Action Nodes**:
- **Completed**: Action completed successfully
- **Failed**: Action failed

**Condition Nodes**:
- **True**: Condition evaluated to true
- **False**: Condition evaluated to false

**Validation Nodes**:
- **Valid**: Validation passed
- **Invalid**: Validation failed

**Control Nodes**:
- **Split**: All outgoing connections use "Next"
- **Join**: Outgoing connection uses "Joined"

### Outcome Requirements

- Each non-Control node must have connections for all required outcomes:
  - Action: Both "Completed" and "Failed" are required
  - Condition: Both "True" and "False" are required
  - Validation: Both "Valid" and "Invalid" are required
- Each semantic outcome can only be used once per node (no duplicate outcomes)
- Control nodes have fixed outcomes (Split: "Next", Join: "Joined")

### Connection Rules

- A connection can only exist between nodes in the same workflow version
- Duplicate connections (same from, to, and outcome) are not allowed
- Connections are copied when creating a new draft version (mapped to new node IDs)
- Outcome must be valid for the source node's category

## Trigger

A **Trigger** defines how a workflow can be started. Triggers are tenant-scoped and can be reused across multiple workflow versions.

### Trigger Types

1. **Manual**: Triggered manually by a user or system
2. **Webhook**: Triggered by an HTTP webhook request
3. **Schedule**: Triggered on a schedule (cron-like)

### Trigger Properties

- **ID**: Unique identifier (UUID)
- **Tenant ID**: Multi-tenant isolation
- **Type**: Trigger type (Manual, Webhook, Schedule)
- **Config**: Type-specific configuration (JSON object)
  - Webhook: URL path, authentication, payload mapping
  - Schedule: Cron expression, timezone

### Trigger Binding

A **Trigger Binding** connects a trigger to a workflow version, making that trigger available to start the workflow.

### Trigger Node Binding

A **Trigger Node Binding** specifies which node(s) in a workflow version are entry points for a trigger. This creates an explicit mapping:
- Trigger → Workflow Version (via Trigger Binding)
- Trigger → Entry Node(s) (via Trigger Node Binding)

### Binding Properties

- **Enabled**: Whether the trigger binding is active
- **Priority**: Optional priority for trigger selection when multiple triggers could start a workflow
- **Entry Nodes**: One or more nodes that serve as entry points for this trigger

### Publishing Requirements

A workflow version can only be published if:
- It has at least one Trigger Binding
- Each Trigger Binding has at least one Trigger Node Binding
- All entry nodes exist and belong to the workflow version

## Workflow Variable

A **Workflow Variable** defines a variable that can be used throughout the workflow execution.

### Variable Properties

- **ID**: Unique identifier (UUID)
- **Key**: Variable identifier (string, must be unique within workflow version)
- **Type**: Variable type (String, Number, Boolean, Object, Array)
- **Initial Value**: Optional default value (JSON string)
- **Description**: Optional description

### Variable Types

- **String**: Text value
- **Number**: Numeric value
- **Boolean**: True/false value
- **Object**: JSON object
- **Array**: JSON array

## Workflow Label

A **Workflow Label** defines a label that can be applied to workflow instances for categorization and filtering.

### Label Properties

- **ID**: Unique identifier (UUID)
- **Key**: Label identifier (string, must be unique within workflow version)
- **Type**: Label type (String, Number, Boolean, Object, Array)
- **Description**: Optional description

Labels are similar to variables but serve a different purpose:
- **Variables**: Used during workflow execution
- **Labels**: Used for categorization and filtering of workflow instances

## Workflow Settings

**Workflow Settings** define execution parameters for a workflow version.

### Settings Properties

- **Max Retries**: Maximum number of retry attempts for failed steps
- **Retry Delay Seconds**: Delay between retry attempts
- **Timeout Seconds**: Optional overall workflow timeout

## Workflow End Nodes

Workflow execution ends implicitly when a node has no outgoing connections. These are called **End Nodes**.

### End Node Rules

- At least one end node must exist in a published workflow
- End nodes cannot be Control nodes (Split or Join)
- End nodes cannot be trigger entry nodes
- End nodes may have incoming connections (allowed)

### Implicit End Semantics

- Execution naturally terminates when reaching a node with no outgoing connections
- Multiple end nodes are allowed (different execution paths can end at different nodes)
- No explicit "End" node type is required

## Compiled Workflow

A **Compiled Workflow** is the execution-ready representation of a published workflow version. It includes:

- All nodes with their configurations and categories
- All connections with their semantic outcomes
- All variables and labels
- All settings
- All triggers with their entry node mappings
- Explicit trigger-to-entry-node mappings

The compiled workflow is what the workflow engine consumes to execute workflows. It's generated on-demand from a published workflow version.

