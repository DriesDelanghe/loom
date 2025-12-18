# Core Concepts

This document explains the fundamental concepts and components in the frontend application.

## Pages

The application has three main pages, each serving a specific purpose in the workflow management lifecycle.

### Workflow List Page

**Route**: `/workflows`

**Purpose**: Overview and entry point for workflow management

**Features**:
- Lists all workflow definitions
- Shows published status and latest version number
- Create new workflow definitions
- Navigate to workflow versions page

**Key Interactions**:
- Click workflow card → Navigate to versions page
- Click "Create Workflow" → Open creation dialog
- View published status badge

### Workflow Versions Page

**Route**: `/workflows/:workflowId`

**Purpose**: Version management for a specific workflow

**Features**:
- Lists all versions of a workflow
- Shows version status (Draft, Published, Archived)
- Create new draft versions
- Delete draft versions
- Navigate to editor for each version

**Key Interactions**:
- Click "Create New Draft" → Create draft from latest published (if exists)
- Click "Edit" (Draft) or "View" (Published) → Navigate to editor
- Click "Delete" (Draft only) → Confirm and delete version

**Layout Copy**: When creating a draft from a published version, the UI automatically copies the layout from the published version.

### Workflow Editor Page

**Route**: `/workflows/:workflowId/versions/:versionId`

**Purpose**: Visual workflow design and configuration

**Features**:
- Visual graph editor with nodes and connections
- Side panels for configuration (nodes, triggers, variables, labels)
- Validation and publishing
- Layout persistence

**Key Interactions**:
- Drag nodes to reposition
- Click nodes to configure
- Connect nodes by dragging from source to target
- Add/remove nodes, connections, triggers
- Validate and publish workflow

## Graph Editor

The graph editor is the core visual interface for designing workflows.

### React Flow Integration

- **Library**: @xyflow/react (React Flow)
- **Layout**: Left-to-right flow (horizontal)
- **Features**: Drag, zoom, pan, minimap, controls

### Node Types

1. **Workflow Nodes**: Regular workflow steps
   - **Action**: Performs actions (blue)
   - **Condition**: Evaluates conditions (purple)
   - **Validation**: Validates data (green)
   - **Split**: Control node for parallel execution (orange)
   - **Join**: Control node for merging branches (orange)
   - Color-coded by type
   - Configurable via side panel
   - Control nodes (Split, Join) are visually distinct with orange borders

2. **Trigger Nodes**: Workflow entry points
   - Manual, Webhook, Schedule
   - Color-coded by type
   - Only have outgoing connections
   - Positioned on the left side of the graph

### Position-Based Connection Outcomes

Connection outcomes are determined by the **connector position** on the source node, not by manual selection:

**Non-Control Nodes** (Action, Condition, Validation):
- **Top connector** (green) = Positive outcome
  - Action → "Completed"
  - Condition → "True"
  - Validation → "Valid"
- **Bottom connector** (red) = Negative outcome
  - Action → "Failed"
  - Condition → "False"
  - Validation → "Invalid"

**Control Nodes**:
- **Split**: All outgoing connections use "Next" outcome (orange connector)
- **Join**: Single outgoing connection uses "Joined" outcome (orange connector)

### Visual Connection Features

- **Green edges**: Positive outcomes (Completed, True, Valid)
- **Red edges**: Negative outcomes (Failed, False, Invalid) - animated
- **Purple edges**: Trigger-to-node connections
- **Orange edges**: Control node connections (Next, Joined)
- **Edge labels**: Show the semantic outcome (e.g., "Completed", "True", "Failed")
- **No manual selection**: Outcomes are inferred from connector position

### Visual Features

- **Handles**: Connection points on nodes
  - **Left side**: Target handles (all nodes can receive connections)
  - **Right side**: Source handles (outcome-specific)
    - Top handle = positive outcome (green)
    - Bottom handle = negative outcome (red)
    - Control nodes have fixed handles (orange)
- **Arrows**: Direction indicators on edges
- **Labels**: Semantic outcome labels on edges (e.g., "Completed", "True", "Failed")
- **End Node Indicators**: Nodes with no outgoing connections show green ring and "• End" label
- **Control Node Indicators**: Split/Join nodes show orange border and "(Control)" label
- **Minimap**: Overview of entire graph
- **Controls**: Zoom, fit view, etc.

## Side Panels

Side panels provide configuration interfaces for different aspects of the workflow.

### Node Configuration Panel

**Trigger**: Click on a workflow node

**Features**:
- View/edit node name
- View/edit node type (Draft only)
- View/edit node configuration (JSON)
- Delete node (Draft only)
- Validation feedback

### Triggers Panel

**Trigger**: Click "Triggers" button or trigger node

**Features**:
- List all triggers bound to workflow version
- Create new triggers
- Bind/unbind triggers to workflow version
- Bind/unbind triggers to entry nodes
- Configure trigger settings (enabled, priority)

### Variables Panel

**Trigger**: Click "Variables" button

**Features**:
- List all workflow variables
- Add new variables
- Edit variable properties (type, initial value, description)
- Delete variables

### Labels Panel

**Trigger**: Click "Labels" button

**Features**:
- List all workflow labels
- Add new labels
- Edit label properties (type, description)
- Delete labels

## State Management

### Server State (React Query)

- **Workflow Definitions**: Cached list of workflows
- **Workflow Versions**: Cached list of versions per workflow
- **Workflow Version Details**: Complete workflow structure
- **Layout Data**: Node positions for workflow version
- **Validation Results**: Cached validation state

**Features**:
- Automatic caching and refetching
- Optimistic updates
- Query invalidation on mutations
- Loading and error states

### Local State (React Hooks)

- **Selected Node**: Currently selected node ID
- **Active Panel**: Which side panel is open
- **Validation State**: Current validation result
- **Node Positions**: React Flow node positions (synced with layout service)
- **Edge State**: React Flow edge state

## Read-Only Mode

### Draft Versions

- **Editable**: All nodes, connections, triggers can be modified
- **Visual Indicators**: No special styling
- **Actions Available**: Add, edit, delete operations

### Published Versions

- **Read-Only**: All properties are immutable
- **Visual Indicators**: Disabled controls, read-only panels
- **Actions Available**: View only, validate, view compiled workflow
- **No Mutations**: Cannot add, edit, or delete anything

## Layout Management

### Position Persistence

- **Automatic Saving**: Positions saved when nodes are dragged
- **Debouncing**: 500ms debounce to reduce API calls
- **Batch Updates**: Multiple position changes batched together
- **Final Save**: Always saves on drag stop

### Layout Loading

- **On Page Load**: Loads layout data for workflow version
- **Race Condition Prevention**: Waits for both workflow data and layout data
- **Fallback**: Default positions if no layout exists
- **Trigger Nodes**: Layout positions saved for trigger nodes too

### Layout Copying

- **Automatic**: When creating draft from published version
- **Key-Based**: Uses node keys to match layouts across versions
- **Error Handling**: Gracefully handles copy failures

## Validation

### Validation Process

1. **User Triggers**: Click "Validate" button
2. **API Call**: Validates workflow version
3. **Display Results**: Shows errors and warnings
4. **Blocking**: Errors prevent publishing
5. **Non-Blocking**: Warnings allow publishing but should be reviewed

### Validation Display

- **Inline Errors**: Shown in relevant panels
- **Summary**: Validation result panel
- **Publish Blocking**: Cannot publish if validation fails

## Publishing

### Publish Workflow

1. **Validate**: System validates workflow
2. **Check Requirements**: Ensures all publishing requirements are met
3. **Publish**: Changes status from Draft to Published
4. **Immutable**: Version becomes read-only
5. **Executable**: Workflow can now be executed by engine

### Publish Requirements

- At least one trigger binding
- Each trigger binding has entry nodes
- All entry nodes exist
- No blocking validation errors

