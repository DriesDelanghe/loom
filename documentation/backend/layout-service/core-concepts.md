# Core Concepts

This document explains the fundamental concepts managed by the Layout Service.

## Node Layout

A **Node Layout** represents the visual position and optional dimensions of a node on the workflow editor canvas.

### Properties

- **Node Key**: Stable identifier for the node (string, max 200 chars)
- **X Coordinate**: Horizontal position on canvas (decimal)
- **Y Coordinate**: Vertical position on canvas (decimal)
- **Width**: Optional node width (decimal, nullable)
- **Height**: Optional node height (decimal, nullable)
- **Updated At**: Timestamp of last update

### Node Keys

- Node keys are stable identifiers that persist across workflow version copies
- When a draft is created from a published version, nodes keep the same keys
- Keys are used instead of node IDs (UUIDs) to maintain layout continuity
- Keys must be unique within a workflow version

### Coordinate System

- Coordinates are stored as decimal values
- Origin (0, 0) is typically top-left of the canvas
- Coordinates are relative to the canvas coordinate system
- No validation is performed on coordinate ranges (UI responsibility)

## Workflow Version Layout

A **Workflow Version Layout** is a container that tracks layout metadata for a workflow version.

### Properties

- **Workflow Version ID**: Reference to the workflow version (UUID)
- **Tenant ID**: Multi-tenant isolation (UUID)
- **Updated At**: Timestamp of last layout update

### Purpose

- Tracks when a workflow version's layout was last modified
- Provides metadata for layout management
- Created automatically when first node layout is saved

## Layout Persistence

### Idempotent Operations

All layout operations are **idempotent**:

- **Upsert**: Create if missing, update if exists
- **Batch Upsert**: Process multiple nodes atomically
- **Copy**: Overwrites existing layouts if present

### Update Behavior

- Updating a node layout updates the `UpdatedAt` timestamp
- Updating any node layout also updates the workflow version layout timestamp
- Deletes are soft (no cascade) - deleting a node layout doesn't affect others

## Multi-Tenancy

- All layout data is tenant-scoped
- Tenant ID is extracted from `X-Tenant-Id` HTTP header
- Default tenant ID is used if header is missing (for development)
- All queries filter by tenant ID to ensure data isolation

## Layout Copying

When creating a new draft workflow version from a published version:

1. **Source Version**: Published version with existing layouts
2. **Target Version**: New draft version
3. **Copy Operation**: Copies all node layouts from source to target
4. **Key Matching**: Uses node keys to match layouts (keys persist across versions)

### Copy Behavior

- **Overwrite**: If target version already has layouts, they are overwritten
- **Key-Based**: Layouts are matched by node key, not node ID
- **Complete Copy**: All node layouts from source are copied
- **Metadata Update**: Target version layout metadata is updated

## Layout Lifecycle

### Initial Layout

- When a workflow version is first opened in the editor, nodes have no saved layouts
- UI assigns default positions
- User can drag nodes to desired positions

### Saving Layout

- Layouts are saved when nodes are dragged
- Can be saved individually or in batches
- Debouncing is typically used to reduce save frequency

### Loading Layout

- When opening a workflow version, layouts are loaded
- Nodes are positioned according to saved layouts
- If no layout exists, default positions are used

### Layout Updates

- Layouts are updated whenever nodes are repositioned
- Updates are idempotent (safe to retry)
- Batch updates are more efficient for multiple nodes

