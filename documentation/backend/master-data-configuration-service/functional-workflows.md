# Functional Workflows

This document describes the key functional workflows and behaviors in the Master Data Configuration Service.

## Schema Lifecycle

### Creating a New Schema

1. **Create Schema**
   - Provide tenant ID, role (Incoming/Master/Outgoing), key, and optional description
   - For Master schemas, DataModelId is optional at creation but required before publishing
   - System creates a new schema with version 1 in Draft status
   - Schema starts empty (no fields)

2. **Define Schema Structure**
   - Add field definitions (Scalar, Object, or Array types)
   - For Object/Array fields, reference other schemas (must be Published when publishing)
   - Configure field properties (required, description)
   - Fields can be added, updated, or removed while in Draft

3. **Configure Validations (Optional)**
   - Create validation specification for the schema
   - Add validation rules (Field, CrossField, Conditional)
   - Configure rule parameters and severity
   - Add validation references for hierarchical validation

4. **Configure Transformations (Optional)**
   - Create transformation specification (Simple or Advanced mode)
   - For Simple mode: Add field-to-field mapping rules
   - For Advanced mode: Build transformation graph with nodes and edges
   - Configure output bindings

5. **Define Business Keys (Master Schemas Only)**
   - Add business key definitions
   - Mark one key as primary
   - Add key fields with ordering and optional normalization
   - At least one business key is required for Master schemas when publishing

6. **Validate Schema**
   - System validates:
     - All field paths are valid
     - Referenced schemas are Published (when publishing)
     - Validation rules reference valid fields
     - Transformation rules reference valid fields
     - Business keys reference valid fields
     - Master schema has DataModelId (when publishing)
   - Validation returns errors and warnings

7. **Publish Schema**
   - System validates the schema meets all publishing requirements
   - If valid, version status changes from Draft to Published
   - Version becomes immutable (read-only)
   - Schema can now be referenced by other schemas and used for runtime operations

### Creating a New Draft from Published Version

1. **Select Published Version**
   - Choose a published version to base the new draft on

2. **Create Draft Version**
   - System creates a new draft version with incremented version number
   - All fields are copied
   - All validation specifications are copied
   - All transformation specifications are copied
   - All business keys are copied
   - All tags are copied

3. **Modify Draft**
   - Edit fields, validations, transformations, keys as needed
   - Changes only affect the draft version
   - Published version remains unchanged

4. **Publish New Version**
   - Validate and publish the new draft
   - New version becomes Published
   - Previous published versions remain Published (multiple published versions can exist)

### Deleting Schemas

1. **Delete Schema Version**
   - Only the latest version of a schema can be deleted
   - Status does not matter (Draft, Published, or Archived can be deleted if latest)
   - Deleting removes the version and all associated data (fields, validations, transformations, keys, tags)

2. **Delete Entire Schema**
   - Deletes all versions of a schema (by Key + Role)
   - Cannot delete if schema is referenced by other schemas
   - System checks for references via ElementSchemaId in field definitions
   - Error message lists all referencing schemas if deletion is blocked

## Field Management

### Adding a Field

1. **Specify Field Details**
   - Provide schema ID
   - Provide unique path (string identifier)
   - Select field type (Scalar, Object, or Array)
   - If Scalar: Select scalar type (String, Integer, Decimal, Boolean, Date, DateTime, Time, Guid)
   - If Object/Array: Select referenced schema (must be Published when publishing)
   - Configure required flag
   - Provide optional description

2. **System Creates Field**
   - Field is added to the schema
   - Field can only be added to Draft schemas
   - Path must be unique within the schema
   - Referenced schema must exist and be Published (when publishing)

### Updating a Field

- Fields can only be updated in Draft schemas
- Can update: path, field type, scalar type, element schema reference, required flag, description
- Published schema fields are read-only

### Removing a Field

- Fields can only be removed from Draft schemas
- Removing a field may break validation rules or transformations that reference it (validation will error)

## Validation Specification Management

### Creating a Validation Specification

1. **Create Validation Spec**
   - Provide schema ID
   - System creates empty validation specification in Draft status

2. **Add Validation Rules**
   - Add rules of type Field, CrossField, or Conditional
   - Configure rule parameters (JSON)
   - Set severity (Error or Warning)
   - Rules can be added, updated, or removed while in Draft

3. **Add Validation References (Optional)**
   - Reference child validation specifications for hierarchical validation
   - Child specs must be Published
   - Field path must exist in schema

4. **Publish Validation Spec**
   - System validates all rules reference valid fields
   - System validates all references point to Published specs
   - If valid, spec status changes to Published
   - Published specs are immutable

### Validation Rule Types

**Field Rules**:
- Validate a single field
- Parameters include field path and validation criteria
- Examples: required check, format validation, range validation

**Cross-Field Rules**:
- Validate relationships between multiple fields
- Parameters include multiple field paths and comparison logic
- Examples: field A must be greater than field B

**Conditional Rules**:
- Validate based on conditions
- Parameters include condition logic and validation rules
- Examples: if field A equals X, then field B must be Y

## Transformation Specification Management

### Creating a Simple Transformation

1. **Create Transformation Spec**
   - Provide source schema ID and target schema ID
   - Select mode: Simple
   - Select type: OneToOne, OneToMany, ManyToOne, ManyToMany
   - System creates transformation specification in Draft status

2. **Add Simple Transform Rules**
   - Add field-to-field mapping rules
   - Specify source path and target path
   - Optionally specify converter ID
   - Configure required flag for target field
   - Rules can be added, updated, or removed while in Draft

3. **Publish Transformation Spec**
   - System validates all source paths exist in source schema
   - System validates all target paths exist in target schema
   - If valid, spec status changes to Published
   - Published specs are immutable

### Creating an Advanced Transformation

1. **Create Transformation Spec**
   - Provide source schema ID and target schema ID
   - Select mode: Advanced
   - System creates transformation specification in Draft status

2. **Build Transformation Graph**
   - Add graph nodes (Source, Map, Filter, Aggregate, Join, Split, Constant, Expression)
   - Connect nodes with edges (typed inputs/outputs)
   - Configure node-specific parameters
   - Ensure graph is acyclic

3. **Configure Output Bindings**
   - Map graph node outputs to target schema fields
   - All target fields must exist
   - Type compatibility is validated

4. **Add Transformation References (Optional)**
   - Reference child transformation specifications for nested transformations
   - Child specs must be Published

5. **Publish Transformation Spec**
   - System validates graph structure (acyclic, all nodes connected)
   - System validates all field paths exist
   - System validates type compatibility
   - If valid, spec status changes to Published
   - Published specs are immutable

### Mode Switching

- Transformation specs start in Simple mode
- Can be upgraded to Advanced mode (one-way upgrade)
- Advanced mode cannot be downgraded to Simple mode
- Mode switch requires explicit confirmation

## Business Key Management

### Adding a Business Key

1. **Create Key Definition**
   - Provide schema ID
   - Provide key name (unique within schema)
   - Mark as primary (if no primary key exists)
   - System creates key definition

2. **Add Key Fields**
   - Add fields to the key
   - Specify field path (must exist in schema)
   - Configure order (significant for key comparison)
   - Optionally configure normalization (uppercase, lowercase, trim, etc.)
   - Fields can be added, removed, or reordered while in Draft

3. **Key Rules**
   - At least one business key is required for Master schemas when publishing
   - Exactly one key must be marked as primary
   - Primary key fields cannot be nullable
   - Primary key fields cannot be arrays or objects (unless explicitly allowed)
   - Field order is significant

### Updating Business Keys

- Keys can only be updated in Draft schemas
- Can update: key name, primary flag, field order
- Cannot update fields in Published schemas
- Published schema keys are read-only

## Tag Management

### Adding Tags

1. **Add Tag to Schema**
   - Provide schema ID and tag value
   - System creates tag
   - Tags are unique within a schema (no duplicates)
   - Tags can be added to any schema version

### Removing Tags

- Tags can be removed from any schema version
- Removing a tag is immediate and permanent

## Schema Reference Management

### Referencing Other Schemas

1. **Create Object or Array Field**
   - Select field type: Object or Array
   - Select referenced schema from available Published schemas
   - Referenced schema must have the same Role
   - Referenced schema must be Published (when publishing)

2. **Reference Validation**
   - When editing: Both Draft and Published schemas can be referenced
   - When publishing: All referenced schemas must be Published
   - System checks for unpublished dependencies
   - If dependencies exist, publishing is blocked with error message

3. **Bulk Publishing**
   - If publishing fails due to unpublished dependencies, system offers to publish all dependencies
   - User can confirm to publish all related schemas
   - System publishes dependencies first, then the main schema

## Publishing Workflow

### Pre-Publishing Validation

Before a schema can be published, the system validates:

1. **Schema Structure**
   - All fields have valid configurations
   - Scalar fields have scalar types
   - Object/Array fields reference Published schemas (same Role)
   - All referenced schemas are Published

2. **Validation Specifications**
   - All validation rules reference valid fields
   - All validation references point to Published specs
   - Field paths in rules exist in schema

3. **Transformation Specifications**
   - All source/target paths exist
   - Graph is acyclic (for Advanced mode)
   - Type compatibility is correct
   - All transformation references point to Published specs

4. **Business Keys (Master Schemas)**
   - At least one business key exists
   - Exactly one primary key exists
   - All key fields exist in schema
   - Primary key fields are not nullable
   - Primary key fields are not arrays/objects

5. **Master Schema Requirements**
   - Master schemas must have a DataModelId

### Publishing Process

1. **Run Validation**
   - System runs all validation checks
   - Returns validation result with errors and warnings

2. **Check Dependencies**
   - System checks for unpublished referenced schemas
   - If dependencies exist, publishing is blocked

3. **Publish Schema**
   - If validation passes, schema status changes to Published
   - Published timestamp is set
   - Schema becomes immutable (read-only)

4. **Publish Related Specifications**
   - Validation specifications are published
   - Transformation specifications are published
   - All become immutable

## Schema Discovery and Filtering

### Schema Overview

- List all schemas for a tenant
- Filter by role (Incoming, Master, Outgoing)
- Filter by status (Draft, Published, Archived)
- Search by key or description
- Display latest version, status, tags, and linked DataModel (for Master schemas)

### Schema Version History

- View all versions of a schema (newest to oldest)
- See version number, status, created date, published date
- Open any version for viewing
- Create new draft from latest published version
- Delete latest version (if applicable)

