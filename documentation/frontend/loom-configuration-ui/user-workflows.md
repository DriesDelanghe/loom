# User Workflows

This document describes the key user workflows and interactions in the frontend application.

## Creating a New Workflow

### Workflow

1. **Navigate to Workflow List**
   - User opens the application
   - Lands on `/workflows` page

2. **Create Workflow Definition**
   - Click "Create Workflow" button
   - Enter workflow name (required)
   - Enter optional description
   - Click "Create"
   - System creates workflow definition
   - User is redirected to workflow versions page

3. **Create Initial Draft**
   - On workflow versions page, click "Create New Draft"
   - System creates empty draft version (version 1)
   - User is redirected to editor

4. **Design Workflow**
   - Add nodes (Action, Condition, Validation, Split, Join)
   - Connect nodes (outcomes determined by connector position)
   - Configure triggers and bind to entry nodes
   - Add variables and labels
   - Configure workflow settings

5. **Validate and Publish**
   - Click "Validate" to check workflow
   - Review errors and warnings
   - Fix any blocking errors
   - Click "Publish" to make workflow executable

## Editing an Existing Workflow

### Workflow

1. **Navigate to Workflow**
   - From workflow list, click on workflow card
   - Navigate to workflow versions page

2. **Open Draft Version**
   - If draft exists, click "Edit" on draft version
   - If no draft, create new draft from published version
   - Navigate to editor

3. **Modify Workflow**
   - Add/remove nodes
   - Modify connections
   - Update node configurations
   - Modify triggers, variables, labels

4. **Save Changes**
   - Changes are saved automatically as user works
   - Node positions saved on drag
   - Node/connection changes saved immediately

5. **Publish New Version**
   - Validate workflow
   - Publish to create new published version
   - Previous published versions remain published

## Working with Nodes

### Adding a Node

1. **Click "Add Node" Button**
   - Button in top-left panel of editor
   - Opens node creation dialog

2. **Configure Node**
   - Enter node key (unique identifier)
   - Enter node name (optional)
   - Select node type (Action, Condition, Validation, Split, Join)
   - Optionally provide initial configuration
     - Split nodes: `{ "maxParallelism": number }`
     - Join nodes: `{ "joinType": "All" | "Any", "cancelRemaining": boolean }`

3. **Node Appears on Canvas**
   - Node is added to graph
   - Positioned at default location
   - User can drag to desired position

### Configuring a Node

1. **Click on Node**
   - Node is selected (highlighted)
   - Node configuration panel opens on right

2. **Edit Properties**
   - Update node name
   - Change node type (Draft only)
   - Edit configuration JSON
   - Save changes

3. **Delete Node** (Draft only)
   - Click "Delete" button in panel
   - Confirm deletion
   - Node and all connections removed

### Positioning Nodes

1. **Drag Node**
   - Click and drag node to new position
   - Position updates in real-time

2. **Auto-Save**
   - Position saved after 500ms of no movement
   - Final position saved on drag stop
   - Layout persisted to Layout Service

## Working with Connections

### Creating a Connection

1. **Select Source Node**
   - Click on source node
   - Hover over output handle (right side)
   - Non-control nodes have two handles:
     - Top handle (green) = positive outcome
     - Bottom handle (red) = negative outcome
   - Control nodes have fixed handles (orange)

2. **Drag to Target**
   - Click and drag from source handle
   - Drag to target node's input handle (left side)
   - Release to create connection

3. **Outcome Determination**
   - Outcome is automatically determined by connector position:
     - **Top connector** (green):
       - Action → "Completed"
       - Condition → "True"
       - Validation → "Valid"
     - **Bottom connector** (red):
       - Action → "Failed"
       - Condition → "False"
       - Validation → "Invalid"
     - **Control nodes**:
       - Split → "Next" (all outgoing)
       - Join → "Joined" (single outgoing)
   - No manual outcome selection - position determines outcome

4. **Validation**
   - System checks for duplicate connections (same from, to, outcome)
   - Prevents connections to trigger nodes (they only output)
   - Ensures required outcomes are present for each node
   - Connection appears on canvas with outcome label

### Changing Connection Outcomes

- **Cannot manually change outcomes**: Outcomes are determined by connector position
- **To change behavior**: Delete the connection and recreate it using the desired connector
- **Visual feedback**: Edge color and label show the outcome (green = positive, red = negative, orange = control)

### Removing a Connection

1. **Click on Edge**
   - Click on the connection line
   - Confirmation dialog appears

2. **Confirm Deletion**
   - Click "Remove" to confirm
   - Connection is removed from graph
   - Changes saved immediately

## Working with Triggers

### Creating a Trigger

1. **Open Triggers Panel**
   - Click "Triggers" button in top panel
   - Triggers panel opens on right

2. **Create Trigger**
   - Click "Create Trigger"
   - Select trigger type (Manual, Webhook, Schedule)
   - Configure trigger settings
   - Save trigger

3. **Bind to Workflow Version**
   - Select trigger from list
   - Click "Bind to Workflow"
   - Configure binding (enabled, priority)
   - Save binding

4. **Bind to Entry Node**
   - Select trigger binding
   - Click "Add Entry Node"
   - Select node from list
   - Node becomes entry point for trigger
   - Connection appears from trigger node to entry node

### Visual Trigger Representation

- **Trigger Node**: Appears as separate node on canvas
- **Trigger Edge**: Purple "Start" edge from trigger to entry node
- **Position**: Trigger nodes positioned on left side of graph

## Working with Variables and Labels

### Adding Variables

1. **Open Variables Panel**
   - Click "Variables" button
   - Variables panel opens

2. **Add Variable**
   - Click "Add Variable"
   - Enter key (unique identifier)
   - Select type (String, Number, Boolean, Object, Array)
   - Optionally provide initial value and description
   - Save variable

### Adding Labels

1. **Open Labels Panel**
   - Click "Labels" button
   - Labels panel opens

2. **Add Label**
   - Click "Add Label"
   - Enter key (unique identifier)
   - Select type (String, Number, Boolean, Object, Array)
   - Optionally provide description
   - Save label

## Validating a Workflow

### Validation Process

1. **Click "Validate" Button**
   - Button in top-right panel
   - Triggers validation API call

2. **View Results**
   - Validation result displayed
   - Errors shown (blocking)
   - Warnings shown (non-blocking)

3. **Fix Issues**
   - Review errors and warnings
   - Fix blocking errors:
     - Missing required outcomes (each non-Control node needs all outcomes)
     - Invalid outcomes for node category
     - Duplicate outcomes on same node
     - Control node structural issues (Split: 1 in, ≥2 out; Join: ≥2 in, 1 out)
     - Missing end nodes or invalid end nodes (cannot be Control or trigger entry nodes)
   - Review warnings (optional)

4. **Re-validate**
   - Click "Validate" again
   - Confirm all errors resolved

## Publishing a Workflow

### Publish Process

1. **Validate First**
   - Ensure workflow is valid
   - All errors resolved

2. **Click "Publish" Button**
   - Button in top-right panel
   - Only enabled for Draft versions

3. **System Validates**
   - Checks publishing requirements
   - Ensures triggers are configured
   - Validates workflow structure

4. **Publish Success**
   - Version status changes to Published
   - Version becomes read-only
   - Workflow is now executable

5. **View Published Version**
   - Version appears in versions list as Published
   - Can be viewed (read-only) in editor
   - Can be used as source for new drafts

## Viewing a Published Workflow

### Read-Only Mode

1. **Navigate to Published Version**
   - From versions page, click "View" on published version
   - Editor opens in read-only mode

2. **Visual Indicators**
   - All controls disabled
   - Panels show read-only state
   - No add/edit/delete actions available

3. **View Only**
   - Can view workflow structure
   - Can view node configurations
   - Can validate workflow
   - Can view compiled workflow

## Deleting a Draft Version

### Deletion Process

1. **Navigate to Versions Page**
   - Go to workflow versions list

2. **Click "Delete" on Draft**
   - Delete button only visible for Draft versions
   - Confirmation dialog appears

3. **Confirm Deletion**
   - Confirm deletion
   - Version and all associated data removed
   - Cannot be undone

## Layout Management

### Automatic Layout Persistence

- **Drag Nodes**: Positions saved automatically
- **Debouncing**: 500ms delay to reduce API calls
- **Batch Updates**: Multiple changes batched
- **Final Save**: Always saves on drag stop

### Layout Loading

- **On Page Load**: Layouts loaded for workflow version
- **Position Restoration**: Nodes positioned according to saved layouts
- **Fallback**: Default positions if no layout exists

### Layout Copying

- **Automatic**: When creating draft from published version
- **Manual**: Can be triggered manually if needed
- **Key-Based**: Uses node keys to match layouts

