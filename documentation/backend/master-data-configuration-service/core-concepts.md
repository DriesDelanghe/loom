# Core Concepts

This document explains the fundamental concepts and entities managed by the Master Data Configuration Service.

## Data Model

A **Data Model** is a top-level container that groups related master data schemas together. It represents a logical domain of master data.

### Properties

- **ID**: Unique identifier (UUID)
- **Tenant ID**: Multi-tenant isolation
- **Key**: Unique identifier within tenant (string, max 200 chars)
- **Name**: Human-readable name (required, max 200 chars)
- **Description**: Optional description (max 2000 chars)
- **Created At**: Timestamp when model was created

### Purpose

Data Models provide organization and grouping for master data schemas. Master data schemas must be associated with a data model when published.

## Data Schema

A **Data Schema** defines the structure and metadata for a data object. Schemas are versioned and can represent incoming data, master data, or outgoing data.

### Schema Roles

Schemas are categorized by their role in the data flow:

1. **Incoming**: Schemas for data entering the system (e.g., external API payloads, file imports)
2. **Master**: Schemas for canonical master data stored in the system
3. **Outgoing**: Schemas for data leaving the system (e.g., API responses, exports)

### Schema Properties

- **ID**: Unique identifier (UUID)
- **Tenant ID**: Multi-tenant isolation
- **Data Model ID**: Optional reference to data model (required for Master schemas when published)
- **Role**: Schema role (Incoming, Master, Outgoing)
- **Key**: Unique identifier within tenant and role (string, max 200 chars)
- **Version**: Version number (integer, auto-incremented per key+role)
- **Status**: Schema status (Draft, Published, Archived)
- **Description**: Optional description (max 2000 chars)
- **Created At**: Timestamp when schema was created
- **Published At**: Timestamp when schema was published (null for Draft/Archived)

### Schema Uniqueness

- Schemas are unique by the combination of `(TenantId, Key, Role)`
- The same key can exist for different roles (e.g., "order" as Incoming, Master, and Outgoing)
- Version numbers are unique per `(Key, Role)` combination

### Schema Status

1. **Draft**: Editable version that can be modified
   - All fields, validations, transformations can be added/removed/modified
   - Cannot be used for runtime operations
   - Can be published to become a Published version

2. **Published**: Immutable, usable version
   - Cannot be modified (read-only)
   - Can be referenced by other schemas
   - Can be used for runtime operations
   - Used as the source when creating new draft versions

3. **Archived**: Historical version that is no longer active
   - Preserved for audit/historical purposes
   - Cannot be modified or used

### Version Numbering

- Versions are auto-incremented integers (1, 2, 3, ...)
- Each schema (Key + Role) maintains its own version sequence
- Version numbers are unique within a (Key, Role) combination

## Field Definition

A **Field Definition** represents a field in a data schema. Fields can be scalar values, objects (references to other schemas), or arrays.

### Field Types

1. **Scalar**: A primitive value (string, number, boolean, date, etc.)
   - Requires a `ScalarType` to be specified
   - Examples: String, Integer, Decimal, Boolean, Date, DateTime, Time, Guid

2. **Object**: A reference to another schema
   - Requires an `ElementSchemaId` pointing to another schema
   - The referenced schema must be Published (when publishing)
   - The referenced schema must have the same Role

3. **Array**: An array of elements
   - **Scalar Array**: Array of scalar values (e.g., `string[]`, `int[]`, `guid[]`)
     - Requires a `ScalarType` to be specified
     - Does NOT require an `ElementSchemaId`
     - Examples: `tags: string[]`, `ids: guid[]`, `prices: decimal[]`
   - **Object Array**: Array of objects that reference another schema
     - Requires an `ElementSchemaId` pointing to another schema
     - Does NOT require a `ScalarType`
     - The referenced schema must be Published (when publishing)
     - The referenced schema must have the same Role
     - Examples: `items: OrderItem[]`, `addresses: Address[]`
   - **Important**: Array fields must have exactly one of `ScalarType` OR `ElementSchemaId`, never both

### Field Properties

- **ID**: Unique identifier (UUID)
- **Data Schema ID**: Parent schema
- **Path**: Field path within schema (string, max 500 chars, unique within schema)
- **Field Type**: Scalar, Object, or Array
- **Scalar Type**: Type of scalar value (if Field Type is Scalar or Array with scalar elements)
- **Element Schema ID**: Reference to another schema (if Field Type is Object or Array with object elements)
- **Required**: Whether field is required (boolean)
- **Description**: Optional description (max 2000 chars)

### Field Paths

- Paths are dot-notation strings (e.g., "customer.email", "order.items")
- Paths must be unique within a schema
- Paths are used to reference fields in validation rules and transformations

## Validation Specification

A **Validation Specification** defines validation rules that apply to a schema. Each schema can have one validation specification.

### Validation Rule Types

1. **Field**: Validates a single field
   - Parameters include field path and validation criteria
   - Examples: required check, format validation, range validation

2. **Cross-Field**: Validates relationships between multiple fields
   - Parameters include multiple field paths and comparison logic
   - Examples: field A must be greater than field B

3. **Conditional**: Validates based on conditions
   - Parameters include condition logic and validation rules
   - Examples: if field A equals X, then field B must be Y

### Validation Rule Properties

- **ID**: Unique identifier (UUID)
- **Validation Spec ID**: Parent validation specification
- **Rule Type**: Field, CrossField, or Conditional
- **Severity**: Error or Warning
- **Parameters**: JSON object containing rule-specific configuration

### Validation References

A **Validation Reference** allows a validation specification to reference child validation specifications, enabling hierarchical validation.

- **Parent Validation Spec ID**: The parent validation specification
- **Field Path**: The field path that triggers child validation
- **Child Validation Spec ID**: The child validation specification to apply

## Transformation Specification

A **Transformation Specification** defines how data is transformed from a source schema to a target schema. Transformations can be simple (direct field mapping) or advanced (graph-based).

### Transformation Modes

1. **Simple Mode**: Direct field-to-field mapping
   - Source field → Target field
   - Optional converter for data transformation
   - Required flag for target fields

2. **Advanced Mode**: Graph-based transformation
   - Visual node-based editor
   - Supports complex transformations (filtering, aggregation, joins, splits)
   - Acyclic graph structure
   - Typed inputs and outputs

### Transformation Spec Properties

- **ID**: Unique identifier (UUID)
- **Source Schema ID**: Source schema for transformation
- **Target Schema ID**: Target schema for transformation
- **Mode**: Simple or Advanced
- **Type**: OneToOne, OneToMany, ManyToOne, ManyToMany
- **Version**: Version number for the transformation spec
- **Status**: Draft, Published, or Archived
- **Description**: Optional description

### Simple Transform Rules

- **Source Path**: Field path in source schema
- **Target Path**: Field path in target schema
- **Converter ID**: Optional converter for data transformation
- **Required**: Whether target field is required
- **Order**: Ordering for multiple rules

### Advanced Transformation Graph

The advanced transformation mode uses a graph-based approach with:

- **Nodes**: Source, Map, Filter, Aggregate, Join, Split, Constant, Expression
- **Edges**: Connections between nodes with typed inputs/outputs
- **Output Bindings**: Mapping from graph outputs to target schema fields
- **References**: Child transformation specifications for nested transformations

## Nested Object and Array Transformations

Nested transformations allow you to transform complex object and array structures by delegating to child transformation specifications. This enables reuse of transformation logic and explicit composition of transformations.

### Transform Reference

A **Transform Reference** connects a field mapping in a parent transformation to a child transformation specification that handles the nested transformation.

### Properties

- **ID**: Unique identifier (UUID)
- **Parent Transformation Spec ID**: The parent transformation specification
- **Source Field Path**: Path to the source field (Object or Array type)
- **Target Field Path**: Path to the target field (Object or Array type)
- **Child Transformation Spec ID**: The child transformation specification to delegate to

### Semantics

When a Transform Reference exists:
- The parent transformation delegates transformation of the specified source field to the child transformation specification
- **For Object/Object-Array fields**: The child transformation's source schema must match the source field's ElementSchemaId, and target schema must match the target field's ElementSchemaId
- **For Scalar Arrays**: Scalar arrays use a virtual schema concept internally. The child transformation's source/target schemas must match the scalar element schemas (determined by ScalarType)
- For Object → Object mappings, the child transformation must have OneToOne cardinality
- For Array → Array mappings, the child transformation must have OneToOne cardinality (applies per-element: one source element → one target element)
- Transform References are **always element-scoped** for arrays, meaning the child transformation is applied to each element

### Scalar Array Transformations

Scalar arrays (e.g., `string[]`, `int[]`) can be transformed in two ways:

1. **Direct Mapping (Simple Mode)**: 
   - `scalar[] → scalar[]` (same scalar type): Direct element-wise copy
   - `object[] → scalar[]`: Field extraction (extract a scalar field from each object)

2. **Structure-Changing (Advanced Mode)**:
   - `scalar[] → object[]`: Requires TransformReference with a child transformation that maps the scalar value to an object
   - Uses a virtual scalar element schema internally for validation

### Explicit Requirement

**Nested transformations are always explicit** - there is no implicit behavior or auto-inference:
- Object and Array fields require an explicit Transform Reference when mapping between schemas
- If a nested transformation is missing, validation will fail at publish time
- The UI assists with discovery and selection, but all transformations must be explicitly configured

### Reusability

Transform References enable transformation reuse:
- A single transformation specification can be used by multiple parent transformations
- Changes to a child transformation affect all parent transformations that reference it
- This promotes consistency and reduces duplication

## Business Key Definition

A **Business Key Definition** defines how an object instance is uniquely identified. Business keys are defined per schema version and are immutable once published.

### Key Properties

- **ID**: Unique identifier (UUID)
- **Data Schema ID**: Parent schema
- **Name**: Key name (unique within schema)
- **Is Primary**: Whether this is the primary key (exactly one per schema)
- **Created At**: Timestamp when key was created

### Key Fields

A **Key Field** represents a field that is part of a business key.

- **ID**: Unique identifier (UUID)
- **Business Key Definition ID**: Parent key definition
- **Field Path**: Path to the field in the schema
- **Order**: Ordering within the key (significant)
- **Normalization**: Optional normalization rule (e.g., uppercase, lowercase, trim)

### Key Rules

- A schema can have multiple business keys
- Exactly one key must be marked as primary
- Key fields must reference existing fields in the schema
- Primary key fields cannot be nullable
- Primary key fields cannot be arrays or objects (unless explicitly allowed)
- Field order is significant for key comparison

## Schema Tag

A **Schema Tag** is a label that can be applied to schemas for categorization and filtering.

### Tag Properties

- **ID**: Unique identifier (UUID)
- **Data Schema ID**: Parent schema
- **Tag**: Tag value (string, max 100 chars, unique within schema)

### Tag Usage

- Tags are used for organization and discovery
- Multiple tags can be applied to a schema
- Tags are displayed in schema overviews
- Tags can be used for filtering and searching

## Schema Flow

A **Schema Flow** represents a data flow relationship between schemas, indicating how data moves through the system.

### Flow Properties

- **ID**: Unique identifier (UUID)
- **Source Schema ID**: Source schema
- **Target Schema ID**: Target schema
- **Flow Type**: Type of flow relationship

## Compiled Transformation Specification

A **Compiled Transformation Specification** is the execution-ready representation of a published transformation specification. It includes:

- All transformation rules (simple or graph-based)
- All field mappings
- All converter references
- All child transformation references
- Complete type information

The compiled specification is what transformation engines consume to perform data transformations at runtime.


