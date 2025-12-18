# Business Rules and Validations

This document describes the business rules and validation logic enforced by the Configuration Service.

## Workflow Version Rules

### Draft Versions

- **Editable**: All properties can be modified
- **Not Executable**: Cannot be executed by the workflow engine
- **Deletable**: Can be deleted
- **Publishable**: Can be published if validation passes

### Published Versions

- **Immutable**: All properties are read-only
- **Executable**: Can be executed by the workflow engine
- **Not Deletable**: Cannot be deleted (preserved for audit/history)
- **Source for Drafts**: Can be used as source when creating new draft versions

### Version Creation Rules

- New draft versions can be created from:
  - Empty (no source version)
  - Published version (copies all data)
- Version numbers are auto-incremented
- Version numbers are unique within a workflow definition

## Node Rules

### Node Creation

- Nodes can only be added to Draft versions
- Node keys must be unique within a workflow version
- Node keys are stable (persist across version copies)
- Node IDs are generated and unique

### Node Modification

- Nodes can only be modified in Draft versions
- Published version nodes are read-only
- Changing node type may require configuration updates

### Node Deletion

- Nodes can only be deleted from Draft versions
- Deleting a node removes all connections to/from that node
- If a node is an entry point for a trigger, trigger node binding must be removed first

## Connection Rules

### Connection Creation

- Connections can only be added to Draft versions
- Both source and target nodes must exist in the workflow version
- Duplicate connections are not allowed:
  - Same from node, to node, and outcome
- Connections must be between nodes in the same workflow version
- Outcome must be valid for the source node's category
- Required outcomes must be present for each non-Control node

### Semantic Outcomes

- **Action Nodes**: Must have both "Completed" and "Failed" outcomes
- **Condition Nodes**: Must have both "True" and "False" outcomes
- **Validation Nodes**: Must have both "Valid" and "Invalid" outcomes
- **Control Nodes**: Use fixed outcomes (Split: "Next", Join: "Joined")
- Each semantic outcome can only be used once per node (for non-Control nodes)

### Connection Deletion

- Connections can only be deleted from Draft versions
- Deleting a connection may create orphaned nodes (validation will warn)
- Deleting a required outcome connection will cause validation errors

## Trigger Rules

### Trigger Creation

- Triggers are tenant-scoped
- Triggers can be reused across multiple workflow versions
- Trigger configuration must be valid for the trigger type

### Trigger Binding Rules

- Trigger bindings can only be created for Draft versions
- Multiple triggers can be bound to the same workflow version
- Each trigger binding can have multiple entry nodes

### Trigger Node Binding Rules

- Entry nodes must exist in the workflow version
- Entry nodes can have incoming connections (allowed)
- Entry nodes must be reachable from the trigger

### Publishing Requirements

A workflow version can only be published if:
- It has at least one trigger binding
- Each trigger binding has at least one trigger node binding
- All entry nodes exist and belong to the workflow version

## Variable and Label Rules

### Variable/Label Creation

- Variables/labels can only be added to Draft versions
- Keys must be unique within a workflow version
- Types must be valid (String, Number, Boolean, Object, Array)

### Variable/Label Modification

- Variables/labels can only be modified in Draft versions
- Published version variables/labels are read-only

### Variable/Label Deletion

- Variables/labels can only be deleted from Draft versions
- Deleting a variable/label may break node configurations that reference it (validation will warn)

## Validation Rules

### Graph Structure Validation

- **Reachability**: All nodes must be reachable from at least one entry point
- **No Orphaned Nodes**: Nodes without connections are allowed but will generate warnings
- **Valid Connections**: All connections must reference existing nodes
- **End Nodes**: At least one end node must exist (node with no outgoing connections)
- **End Node Constraints**: End nodes cannot be Control nodes or trigger entry nodes

### Node Configuration Validation

- **Required Fields**: Each node type has required configuration fields
- **Type Validation**: Configuration values must match expected types
- **Value Validation**: Configuration values must be within valid ranges/formats
- **Control Node Configs**:
  - Split: maxParallelism must be a positive integer
  - Join: joinType must be "All" or "Any", cancelRemaining must be boolean

### Trigger Validation

- **Binding Exists**: At least one trigger binding must exist
- **Entry Nodes**: Each trigger binding must have at least one entry node
- **Node Existence**: All entry nodes must exist in the workflow version

### Outcome Validation

- **Valid Outcomes**: All connection outcomes must be valid for their source node's category
- **Required Outcomes**: Each non-Control node must have connections for all required outcomes
- **No Duplicates**: Each semantic outcome can only be used once per node (for non-Control nodes)
- **Control Node Constraints**:
  - Split: Exactly 1 incoming, at least 2 outgoing, all use "Next"
  - Join: At least 2 incoming, exactly 1 outgoing, uses "Joined"

### Variable/Label Validation

- **Reference Validation**: Variables/labels referenced in node configurations must exist
- **Type Matching**: Variable types must match expected types in node configurations

## Publishing Validation

Before a workflow version can be published, the system validates:

1. **Graph Completeness**
   - All nodes are reachable
   - No critical structural issues

2. **Configuration Validity**
   - All nodes have valid configurations
   - All variables/labels are properly defined

3. **Trigger Configuration**
   - At least one trigger binding exists
   - Each trigger binding has entry nodes
   - All entry nodes exist

4. **No Blocking Errors**
   - All validation errors must be resolved
   - Warnings are allowed but should be reviewed

## Compilation Rules

### Compilation Requirements

- Only Published versions can be compiled
- Compiled workflow includes all execution metadata
- Compiled workflow is optimized for runtime performance

### Compilation Output

- **Nodes**: All nodes with configurations
- **Connections**: All connections with types
- **Variables**: All variable definitions
- **Labels**: All label definitions
- **Settings**: Execution settings
- **Triggers**: All triggers with entry node mappings

## Data Integrity Rules

### Foreign Key Constraints

- Nodes belong to workflow versions (cascade delete)
- Connections belong to workflow versions (cascade delete)
- Trigger bindings belong to workflow versions (cascade delete)
- Trigger node bindings belong to trigger bindings (cascade delete)
- Variables/labels belong to workflow versions (cascade delete)

### Unique Constraints

- Node keys are unique within a workflow version
- Variable/label keys are unique within a workflow version
- Version numbers are unique within a workflow definition
- Connections are unique by (from, to, outcome) within a workflow version

### Cascade Deletes

- Deleting a workflow version deletes all associated nodes, connections, triggers, variables, labels
- Deleting a trigger binding deletes all associated trigger node bindings
- Deleting a node deletes all connections to/from that node (if not prevented by trigger bindings)

