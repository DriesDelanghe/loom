# Data Model

This document provides an overview of the data model used by the Configuration Service.

## Database Schema

The service uses PostgreSQL as its database. The schema is managed through Flyway migrations.

### Core Tables

#### WorkflowDefinitions
- **Id**: UUID (Primary Key)
- **TenantId**: UUID
- **Name**: String (max 200 chars, required)
- **Description**: String (max 2000 chars, nullable)
- **IsArchived**: Boolean
- **CreatedAt**: Timestamp
- **UpdatedAt**: Timestamp

**Indexes**: (TenantId, Name)

#### WorkflowVersions
- **Id**: UUID (Primary Key)
- **DefinitionId**: UUID (Foreign Key to WorkflowDefinitions)
- **Version**: Integer
- **Status**: Enum (Draft, Published, Archived)
- **CreatedAt**: Timestamp
- **CreatedBy**: String
- **PublishedAt**: Timestamp (nullable)
- **PublishedBy**: String (nullable)

**Indexes**: (DefinitionId, Version) - Unique

#### Nodes
- **Id**: UUID (Primary Key)
- **WorkflowVersionId**: UUID (Foreign Key to WorkflowVersions)
- **Key**: String (max 200 chars, required)
- **Name**: String (max 200 chars, nullable)
- **Type**: Enum (Action, Condition, Validation, Split, Join)
- **ConfigJson**: JSON (nullable)
- **CreatedAt**: Timestamp

**Indexes**: (WorkflowVersionId, Key) - Unique

**Note**: Node category (Action, Condition, Validation, Control) is derived from node type and not stored in the database.

#### Connections
- **Id**: UUID (Primary Key)
- **WorkflowVersionId**: UUID (Foreign Key to WorkflowVersions)
- **FromNodeId**: UUID (Foreign Key to Nodes)
- **ToNodeId**: UUID (Foreign Key to Nodes)
- **Outcome**: String (max 50 chars, required) - Semantic outcome (e.g., "Completed", "Failed", "True", "False", "Valid", "Invalid", "Next", "Joined")
- **Order**: Integer (nullable)

**Indexes**: (WorkflowVersionId, FromNodeId, ToNodeId, Outcome)

#### Triggers
- **Id**: UUID (Primary Key)
- **TenantId**: UUID
- **Type**: Enum (Manual, Webhook, Schedule)
- **ConfigJson**: JSON (nullable)
- **CreatedAt**: Timestamp

#### TriggerBindings
- **Id**: UUID (Primary Key)
- **TriggerId**: UUID (Foreign Key to Triggers)
- **WorkflowVersionId**: UUID (Foreign Key to WorkflowVersions)
- **Enabled**: Boolean
- **Priority**: Integer (nullable)

#### TriggerNodeBindings
- **Id**: UUID (Primary Key)
- **TriggerBindingId**: UUID (Foreign Key to TriggerBindings)
- **EntryNodeId**: UUID (Foreign Key to Nodes)
- **Order**: Integer

#### WorkflowVariables
- **Id**: UUID (Primary Key)
- **WorkflowVersionId**: UUID (Foreign Key to WorkflowVersions)
- **Key**: String (max 200 chars, required)
- **Type**: Enum (String, Number, Boolean, Object, Array)
- **InitialValueJson**: JSON (nullable)
- **Description**: String (max 2000 chars, nullable)

**Indexes**: (WorkflowVersionId, Key) - Unique

#### WorkflowLabelDefinitions
- **Id**: UUID (Primary Key)
- **WorkflowVersionId**: UUID (Foreign Key to WorkflowVersions)
- **Key**: String (max 200 chars, required)
- **Type**: Enum (String, Number, Boolean, Object, Array)
- **Description**: String (max 2000 chars, nullable)

**Indexes**: (WorkflowVersionId, Key) - Unique

#### WorkflowSettings
- **Id**: UUID (Primary Key)
- **WorkflowVersionId**: UUID (Foreign Key to WorkflowVersions, Unique)
- **MaxRetries**: Integer
- **RetryDelaySeconds**: Integer
- **TimeoutSeconds**: Integer (nullable)

## Domain Model

The domain model represents the business entities and their relationships.

### Workflow Definition
- Contains multiple Workflow Versions
- Has tenant isolation
- Can be archived

### Workflow Version
- Belongs to a Workflow Definition
- Contains Nodes, Connections, Variables, Labels, Settings
- Has Trigger Bindings
- Has status (Draft, Published, Archived)

### Node
- Belongs to a Workflow Version
- Has a stable Key
- Has a Type and Configuration
- Can be source/target of Connections
- Can be an entry point for Triggers

### Connection
- Belongs to a Workflow Version
- Connects two Nodes
- Has a semantic Outcome (string) that indicates execution path
- Outcome must be valid for source node's category
- Has optional Order

### Trigger
- Tenant-scoped
- Has a Type and Configuration
- Can be bound to multiple Workflow Versions

### Trigger Binding
- Connects a Trigger to a Workflow Version
- Has Enabled status and optional Priority
- Contains Trigger Node Bindings

### Trigger Node Binding
- Connects a Trigger Binding to an Entry Node
- Specifies which node(s) are entry points for a trigger
- Has optional Order

### Workflow Variable
- Belongs to a Workflow Version
- Has a Key, Type, and optional Initial Value
- Used during workflow execution

### Workflow Label
- Belongs to a Workflow Version
- Has a Key and Type
- Used for categorization and filtering

### Workflow Settings
- Belongs to a Workflow Version (one-to-one)
- Defines execution parameters (retries, delays, timeouts)

## Persistence Layer

The persistence layer uses Entity Framework Core with PostgreSQL.

### Entity Classes

Entity classes in `Loom.Services.Configuration.Domain.Persistence`:
- `WorkflowDefinitionEntity`
- `WorkflowVersionEntity`
- `NodeEntity`
- `ConnectionEntity`
- `TriggerEntity`
- `TriggerBindingEntity`
- `TriggerNodeBindingEntity`
- `WorkflowVariableEntity`
- `WorkflowLabelDefinitionEntity`
- `WorkflowSettingsEntity`

### Domain Mapping

Entities have `ToDomain()` methods that convert persistence entities to domain models, and `FromDomain()` static methods for the reverse conversion.

### DbContext

`ConfigurationDbContext` manages the database context and entity configurations.

## Relationships

### One-to-Many
- Workflow Definition → Workflow Versions
- Workflow Version → Nodes
- Workflow Version → Connections
- Workflow Version → Variables
- Workflow Version → Labels
- Workflow Version → Trigger Bindings
- Trigger → Trigger Bindings
- Trigger Binding → Trigger Node Bindings

### One-to-One
- Workflow Version → Workflow Settings

### Many-to-Many (via Junction)
- Trigger → Workflow Version (via TriggerBinding)
- Trigger → Node (via TriggerBinding + TriggerNodeBinding)

## Data Integrity

### Constraints
- Foreign key constraints ensure referential integrity
- Unique constraints prevent duplicates
- Check constraints validate enum values

### Cascade Deletes
- Deleting a Workflow Definition cascades to Workflow Versions
- Deleting a Workflow Version cascades to Nodes, Connections, Variables, Labels, Settings, Trigger Bindings
- Deleting a Trigger Binding cascades to Trigger Node Bindings
- Deleting a Node cascades to Connections (with validation for trigger bindings)

## Multi-Tenancy

- **Workflow Definitions**: Tenant-scoped (TenantId)
- **Triggers**: Tenant-scoped (TenantId)
- **Workflow Versions**: Inherit tenant from Workflow Definition

All queries are filtered by TenantId to ensure data isolation.

