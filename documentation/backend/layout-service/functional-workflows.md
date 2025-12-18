# Functional Workflows

This document describes the key functional workflows and behaviors in the Layout Service.

## Saving Node Positions

### Single Node Save

1. **User Drags Node**
   - User drags a node to a new position on the canvas
   - UI captures the new X, Y coordinates

2. **Save Request**
   - UI sends PUT request with node key and coordinates
   - Optional: Include width and height if available

3. **System Processes**
   - System upserts the layout (create if missing, update if exists)
   - Updates workflow version layout metadata timestamp
   - Returns success status

### Batch Node Save

1. **Multiple Nodes Moved**
   - User drags multiple nodes or layout algorithm repositions nodes
   - UI collects all position changes

2. **Batch Save Request**
   - UI sends PUT request with array of node layouts
   - All nodes are updated in a single transaction

3. **System Processes**
   - System upserts all node layouts atomically
   - Updates workflow version layout metadata timestamp
   - Returns success status

### Debounced Saves

- UI typically implements debouncing (e.g., 500ms delay)
- Reduces number of save requests during dragging
- Final save on drag stop ensures all positions are persisted

## Loading Node Positions

### Initial Load

1. **Open Workflow Version**
   - User opens a workflow version in the editor
   - UI requests layout data for the workflow version

2. **System Retrieves Layout**
   - System queries all node layouts for the workflow version
   - Returns array of node layouts keyed by node key

3. **UI Applies Layout**
   - UI matches node layouts to nodes by key
   - Positions nodes according to saved layouts
   - Nodes without saved layouts use default positions

### Layout Not Found

- If no layout exists for a workflow version, empty array is returned
- UI assigns default positions (e.g., grid layout, horizontal flow)
- User can then position nodes and save layouts

## Copying Layout Between Versions

### Copy-Forward Workflow

1. **Create Draft from Published**
   - User creates a new draft version from a published version
   - Configuration Service copies workflow structure

2. **Copy Layout (Optional)**
   - UI can optionally copy layout from source version
   - Sends POST request to copy layout

3. **System Copies Layout**
   - System copies all node layouts from source to target
   - Uses node keys to match layouts (keys persist across versions)
   - Overwrites any existing layouts in target version

4. **UI Applies Copied Layout**
   - UI loads the copied layout
   - Nodes are positioned according to source version layout

### Copy Behavior

- **Complete Copy**: All node layouts are copied
- **Key-Based Matching**: Layouts matched by node key, not node ID
- **Overwrite**: Existing layouts in target are overwritten
- **Idempotent**: Safe to call multiple times

## Deleting Node Layouts

### Single Node Delete

1. **Node Removed from Workflow**
   - User removes a node from the workflow (Configuration Service)
   - UI can optionally clean up the layout

2. **Delete Request**
   - UI sends DELETE request with node key

3. **System Deletes**
   - System removes the node layout
   - Workflow version layout metadata is not affected

### Batch Cleanup

- UI can delete multiple node layouts if nodes are removed
- Typically done asynchronously (not blocking workflow operations)
- Orphaned layouts (nodes that no longer exist) are harmless but can be cleaned up

## Layout Synchronization

### Race Condition Prevention

- Layout service is designed to handle concurrent updates
- Upsert operations are idempotent (safe to retry)
- Last write wins (no conflict resolution needed)

### Consistency

- Layout data is eventually consistent
- No strict consistency requirements (UI can handle missing/stale layouts)
- Default positions provide fallback

## Integration with Configuration Service

### Decoupled Design

- Layout Service is **independent** from Configuration Service
- No direct coupling or shared database
- Communication via HTTP API only

### Workflow

1. **Configuration Service**: Manages workflow structure (nodes, connections, etc.)
2. **Layout Service**: Manages visual layout (positions)
3. **Frontend**: Orchestrates both services

### Example: Creating Draft Version

1. Frontend calls Configuration Service to create draft
2. Frontend calls Layout Service to copy layout (optional)
3. Frontend loads both workflow structure and layout
4. Frontend renders workflow with saved positions

## Error Handling

### Missing Layouts

- If layout doesn't exist, empty array is returned
- UI handles gracefully with default positions
- Not considered an error condition

### Invalid Node Keys

- Invalid node keys are ignored (no error thrown)
- UI should validate node keys before saving
- Orphaned layouts (keys that don't match nodes) are harmless

### Network Errors

- UI should handle network failures gracefully
- Retry logic can be implemented
- Layout saves can be queued and retried later

