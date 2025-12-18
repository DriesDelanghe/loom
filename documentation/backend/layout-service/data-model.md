# Data Model

This document provides an overview of the data model used by the Layout Service.

## Database Schema

The service uses PostgreSQL as its database. The schema is managed through Flyway migrations.

### Core Tables

#### WorkflowVersionLayouts
- **TenantId**: UUID (Part of Primary Key)
- **WorkflowVersionId**: UUID (Part of Primary Key)
- **UpdatedAt**: Timestamp

**Primary Key**: (TenantId, WorkflowVersionId)  
**Indexes**: WorkflowVersionId

**Purpose**: Tracks layout metadata for workflow versions. Created automatically when first node layout is saved.

#### WorkflowNodeLayouts
- **TenantId**: UUID (Part of Primary Key)
- **WorkflowVersionId**: UUID (Part of Primary Key)
- **NodeKey**: String (max 200 chars, Part of Primary Key)
- **X**: Decimal(18,2) - X coordinate
- **Y**: Decimal(18,2) - Y coordinate
- **Width**: Decimal(18,2) - Optional node width (nullable)
- **Height**: Decimal(18,2) - Optional node height (nullable)
- **UpdatedAt**: Timestamp

**Primary Key**: (TenantId, WorkflowVersionId, NodeKey)  
**Indexes**: (WorkflowVersionId, NodeKey)

**Purpose**: Stores individual node positions and dimensions.

## Domain Model

The domain model is simple and focused on layout data.

### Node Layout
- **Node Key**: Stable identifier (string)
- **X, Y**: Position coordinates (decimal)
- **Width, Height**: Optional dimensions (decimal, nullable)

### Workflow Version Layout
- **Workflow Version ID**: Reference to workflow version (UUID)
- **Updated At**: Last modification timestamp

## Persistence Layer

The persistence layer uses Entity Framework Core with PostgreSQL.

### Entity Classes

Entity classes in `Loom.Services.Layout.Domain.Persistence`:
- `WorkflowVersionLayoutEntity`
- `WorkflowNodeLayoutEntity`

### DbContext

`LayoutDbContext` manages the database context and entity configurations.

## Relationships

### One-to-Many
- Workflow Version Layout → Node Layouts (conceptual, not enforced by foreign key)

### Key Design

- **Composite Primary Keys**: All tables use composite keys including TenantId
- **No Foreign Keys**: No foreign key constraints to Configuration Service (decoupled design)
- **Node Keys**: Use string keys instead of UUIDs for stability across versions

## Data Integrity

### Constraints
- **Primary Keys**: Ensure uniqueness of layouts
- **Node Key Length**: Max 200 characters
- **Decimal Precision**: 18 digits, 2 decimal places

### Unique Constraints
- Node layouts are unique by (TenantId, WorkflowVersionId, NodeKey)
- Workflow version layouts are unique by (TenantId, WorkflowVersionId)

### No Cascade Deletes
- Deleting a node layout doesn't cascade to other layouts
- Deleting a workflow version layout doesn't cascade to node layouts
- This allows for independent cleanup and orphaned layout handling

## Multi-Tenancy

- **All Tables**: Tenant-scoped (TenantId in primary key)
- **Queries**: All queries filter by TenantId
- **Isolation**: Complete data isolation between tenants

## Node Key Strategy

### Why Node Keys Instead of Node IDs?

1. **Stability**: Keys persist across workflow version copies
2. **Copy-Forward**: When creating draft from published version, nodes keep same keys
3. **Layout Continuity**: Layouts can be easily copied between versions using keys
4. **Decoupling**: No dependency on Configuration Service node IDs

### Node Key Format

- **Workflow Nodes**: Use the node key from Configuration Service (e.g., "action-1", "condition-1")
- **Trigger Nodes**: Use format "trigger-{triggerBindingId}" (e.g., "trigger-abc-123")
- **Uniqueness**: Keys must be unique within a workflow version

## Data Lifecycle

### Creation
- Node layouts are created when first saved
- Workflow version layout is created automatically when first node layout is saved

### Updates
- Node layouts are updated via upsert operations
- Workflow version layout timestamp is updated when any node layout is updated

### Deletion
- Node layouts can be deleted individually
- No cascade deletes (orphaned layouts are harmless)
- Workflow version layout is not deleted when all node layouts are removed

## Performance Considerations

### Indexing
- **WorkflowVersionId**: Indexed for fast queries by version
- **NodeKey**: Indexed for fast lookups by key
- **Composite Index**: (WorkflowVersionId, NodeKey) for efficient queries

### Query Patterns
- **Load All**: Query all node layouts for a workflow version (common)
- **Load Single**: Query single node layout by key (less common)
- **Batch Upsert**: Insert/update multiple layouts in single transaction (efficient)

### Scalability
- **Partitioning**: Could be partitioned by TenantId or WorkflowVersionId if needed
- **Archiving**: Old layouts could be archived but currently preserved indefinitely
- **Cleanup**: Orphaned layouts (nodes that no longer exist) can be cleaned up periodically

