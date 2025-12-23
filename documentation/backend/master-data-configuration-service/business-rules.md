# Business Rules and Validations

This document describes the business rules and validation logic enforced by the Master Data Configuration Service.

## Schema Rules

### Draft Schemas

- **Editable**: All properties can be modified (fields, validations, transformations, keys)
- **Not Usable**: Cannot be referenced by other schemas or used for runtime operations
- **Deletable**: Can be deleted (only latest version)
- **Publishable**: Can be published if validation passes

### Published Schemas

- **Immutable**: All properties are read-only
- **Referenceable**: Can be referenced by other schemas (via Object/Array fields)
- **Not Deletable**: Cannot be deleted (preserved for audit/history)
- **Source for Drafts**: Can be used as source when creating new draft versions

### Schema Creation Rules

- New schemas can be created with:
  - Tenant ID, Role (Incoming/Master/Outgoing), Key, optional Description
  - For Master schemas, DataModelId is optional at creation but required before publishing
- Schema keys are unique by (TenantId, Key, Role)
- The same key can exist for different roles (e.g., "order" as Incoming, Master, and Outgoing)
- Version numbers are auto-incremented per (Key, Role) combination

### Schema Deletion Rules

- **Version Deletion**: Only the latest version can be deleted (regardless of status)
- **Schema Deletion**: Can delete all versions of a schema (by Key + Role)
- **Reference Check**: Cannot delete a schema if it is referenced by other schemas
- **Reference Error**: Error message lists all schemas that reference the schema being deleted

## Field Rules

### Field Creation

- Fields can only be added to Draft schemas
- Field paths must be unique within a schema
- **Scalar fields**: Must have a scalar type specified, cannot have ElementSchemaId
- **Object fields**: Must reference an existing schema (ElementSchemaId), cannot have ScalarType
- **Array fields**: Must have exactly one of:
  - **Scalar Array**: ScalarType specified (e.g., `string[]`, `int[]`), no ElementSchemaId
  - **Object Array**: ElementSchemaId specified, no ScalarType
- Array fields cannot have both ScalarType and ElementSchemaId
- Array fields cannot have neither ScalarType nor ElementSchemaId
- Referenced schemas (for Object/Object-Array fields) must be Published (when publishing)
- Referenced schemas must have the same Role

### Field Modification

- Fields can only be modified in Draft schemas
- Published schema fields are read-only
- Can update: path, field type, scalar type, element schema reference, required flag, description

### Field Deletion

- Fields can only be deleted from Draft schemas
- Deleting a field may break validation rules or transformations (validation will error)

### Field Path Rules

- Paths are dot-notation strings (e.g., "customer.email", "order.items")
- Paths must be unique within a schema
- Paths are used to reference fields in validation rules and transformations

## Validation Specification Rules

### Validation Spec Creation

- Validation specs can only be created for Draft schemas
- Each schema can have one validation specification
- Validation specs start in Draft status

### Validation Rule Rules

- Rules can only be added/modified/removed in Draft validation specs
- Published validation specs are immutable
- Rule parameters must be valid JSON
- Field paths in rules must exist in the schema
- Validation references must point to Published validation specs

### Validation Rule Types

- **Field Rules**: Must reference a valid field path
- **Cross-Field Rules**: Must reference multiple valid field paths
- **Conditional Rules**: Must have valid condition logic and validation rules

### Publishing Requirements

A validation specification can only be published if:
- All rules reference valid fields
- All validation references point to Published specs
- All field paths exist in the schema

## Transformation Specification Rules

### Transformation Spec Creation

- Transformation specs can only be created for Draft schemas
- Must specify source schema and target schema
- Source and target schemas must exist
- Transformation specs start in Draft status

### Simple Mode Rules

- Source paths must exist in source schema
- Target paths must exist in target schema
- Type compatibility is validated:
  - **Allowed in Simple Mode**:
    - `scalar → scalar`: Direct mapping
    - `scalar[] → scalar[]`: Direct element-wise copy (same scalar type)
    - `object[] → scalar[]`: Field extraction (extract scalar field from each object)
    - `object[] → object[]`: Same schema mapping (requires TransformReference)
  - **Blocked in Simple Mode** (requires Advanced Mode):
    - `scalar[] → object[]`: Structure-changing transformation (requires TransformReference)
    - `object[] → object[]`: Different schema mapping without TransformReference
    - `scalar → object/array`: Structure-changing transformation
- Rules can be added/modified/removed in Draft specs

### Advanced Mode Rules

- Graph must be acyclic
- All nodes must be connected
- All edges must reference existing nodes
- Output bindings must reference existing nodes and target fields
- Type compatibility is validated for all connections
- Transformation references must point to Published specs

### Transform Reference Rules

- References can only be added to Draft transformation specs
- Source and target field paths must exist in their respective schemas
- Source and target fields must be Object or Array type
- Source and target fields must have the same type (Object/Object or Array/Array)
- **For Object/Object-Array fields**: Both source and target fields must have ElementSchemaId
- **For Scalar Arrays**: Fields use ScalarType (virtual schema concept used internally)
- Child transformation spec must exist and be Published
- **Schema Matching**:
  - For Object/Object-Array: Child transformation source/target schemas must match ElementSchemaId
  - For Scalar Arrays: Child transformation source/target schemas must match virtual scalar element schemas (determined by ScalarType)
- For Object → Object mappings: Child transformation must have OneToOne cardinality
- For Array → Array mappings: Child transformation must have OneToOne cardinality (applies per-element: one source element → one target element)
- No implicit behavior: All nested transformations must be explicitly defined

### Mode Switching

- Simple mode can be upgraded to Advanced mode (one-way)
- Advanced mode cannot be downgraded to Simple mode
- Mode switch requires explicit confirmation

### Publishing Requirements

A transformation specification can only be published if:
- All source/target paths exist (Simple mode)
- Graph is acyclic and valid (Advanced mode)
- All output bindings reference valid fields
- All transformation references point to Published specs

## Business Key Rules

### Key Creation

- Keys can only be added to Draft schemas
- Key names must be unique within a schema
- At least one business key is required for Master schemas when publishing
- Exactly one key must be marked as primary

### Key Field Rules

- Key fields must reference existing fields in the schema
- Field order is significant for key comparison
- Primary key fields cannot be nullable
- Primary key fields cannot be arrays or objects (unless explicitly allowed)
- No duplicate field paths in the same key

### Key Modification

- Keys can only be modified in Draft schemas
- Can update: key name, primary flag, field order
- Can add/remove/reorder key fields
- Published schema keys are immutable

### Publishing Requirements

A Master schema can only be published if:
- At least one business key exists
- Exactly one primary key exists
- All key fields exist in schema
- Primary key fields are not nullable
- Primary key fields are not arrays/objects

## Schema Reference Rules

### Reference Creation

- Object/Array fields can reference other schemas
- Referenced schemas must have the same Role
- When editing: Both Draft and Published schemas can be referenced
- When publishing: All referenced schemas must be Published

### Reference Validation

- System checks for unpublished dependencies before publishing
- If dependencies exist, publishing is blocked
- Error message lists all unpublished dependencies
- System offers bulk publishing of dependencies

### Reference Deletion

- Cannot delete a schema if it is referenced by other schemas
- Error message lists all referencing schemas
- Must remove all references before deletion

## Tag Rules

### Tag Creation

- Tags can be added to any schema version
- Tags are unique within a schema (no duplicates)
- Tag values are trimmed and validated

### Tag Deletion

- Tags can be removed from any schema version
- Tag removal is immediate and permanent

## Publishing Validation

Before a schema can be published, the system validates:

1. **Schema Structure**
   - All fields have valid configurations
   - Scalar fields have scalar types (no ElementSchemaId)
   - Object fields have ElementSchemaId (no ScalarType)
   - Array fields have exactly one of ScalarType OR ElementSchemaId (never both, never neither)
   - Object/Object-Array fields reference Published schemas (same Role)
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

6. **Dependency Check**
   - All referenced schemas are Published
   - If dependencies are unpublished, publishing is blocked

## Data Integrity Rules

### Foreign Key Constraints

- Fields belong to schemas (cascade delete)
- Validation specs belong to schemas (cascade delete)
- Transformation specs belong to schemas (cascade delete)
- Business keys belong to schemas (cascade delete)
- Key fields belong to business keys (cascade delete)
- Tags belong to schemas (cascade delete)

### Unique Constraints

- Schema keys are unique by (TenantId, Key, Role)
- Field paths are unique within a schema
- Key names are unique within a schema
- Tag values are unique within a schema
- Validation rule parameters must be valid JSON
- Transformation graph must be acyclic

### Cascade Deletes

- Deleting a schema deletes all associated fields, validations, transformations, keys, tags
- Deleting a validation spec deletes all associated rules and references
- Deleting a transformation spec deletes all associated rules, nodes, edges, bindings, references
- Deleting a business key deletes all associated key fields


