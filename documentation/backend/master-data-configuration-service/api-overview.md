# API Overview

The Master Data Configuration Service exposes an HTTP REST API for managing data schemas and their configurations.

Base path: `/api`

## Data Model Endpoints

### Create Data Model
- **POST** `/datamodels`
- Creates a new data model
- Request body: `{ tenantId, key, name, description? }`
- Returns: Data model ID

## Schema Endpoints

### Get Schemas
- **GET** `/schemas?tenantId={tenantId}&role={role?}&status={status?}`
- Lists all schemas for a tenant
- Optional filters: role (Incoming/Master/Outgoing), status (Draft/Published/Archived)
- Returns: Array of schema summaries (includes tags)

### Get Schema Details
- **GET** `/schemas/{id}`
- Gets complete details of a schema version
- Returns: Schema with fields, tags, key definitions

### Get Schema Graph
- **GET** `/schemas/{id}/graph`
- Gets the reference graph for a schema
- Shows which schemas this schema references and which schemas reference it
- Returns: Graph with nodes and edges

### Create Schema
- **POST** `/schemas`
- Creates a new schema
- Request body: `{ tenantId, dataModelId?, role, key, description? }`
- Returns: Schema ID

### Add Field
- **POST** `/schemas/{id}/fields`
- Adds a field to a schema
- Request body: `{ path, fieldType, scalarType?, elementSchemaId?, required, description? }`
- **Field Type Rules**:
  - **Scalar**: Requires `scalarType`, cannot have `elementSchemaId`
  - **Object**: Requires `elementSchemaId`, cannot have `scalarType`
  - **Array**: Requires exactly one of:
    - `scalarType` (for scalar arrays, e.g., `string[]`, `int[]`)
    - `elementSchemaId` (for object arrays, e.g., `OrderItem[]`)
  - Array fields cannot have both `scalarType` and `elementSchemaId`
- Returns: Field ID

### Update Field
- **PUT** `/schemas/fields/{fieldId}`
- Updates a field definition
- Request body: `{ path?, fieldType?, scalarType?, elementSchemaId?, required?, description? }`
- Returns: Success status

### Remove Field
- **DELETE** `/schemas/fields/{fieldId}`
- Removes a field from a schema
- Returns: Success status

### Publish Schema
- **POST** `/schemas/{id}/publish`
- Publishes a draft schema (makes it immutable and usable)
- Request body: `{ publishedBy }`
- Returns: Success status

### Validate Schema
- **GET** `/schemas/{id}/validate`
- Validates a schema
- Returns: Validation result with errors and warnings

### Get Unpublished Dependencies
- **GET** `/schemas/{id}/unpublished-dependencies`
- Gets list of unpublished schemas that this schema references
- Returns: Array of unpublished dependency schemas

### Publish Related Schemas
- **POST** `/schemas/{id}/publish-related`
- Publishes all related schemas that are still in Draft
- Request body: `{ publishedBy, relatedSchemaIds[] }`
- Returns: Success status

### Delete Schema Version
- **DELETE** `/schemas/{id}`
- Deletes a schema version (only latest version can be deleted)
- Returns: Success status

### Delete Schema
- **DELETE** `/schemas?key={key}&role={role}&tenantId={tenantId}`
- Deletes all versions of a schema (by Key + Role)
- Cannot delete if schema is referenced by other schemas
- Returns: Success status

### Add Tag
- **POST** `/schemas/{id}/tags`
- Adds a tag to a schema
- Request body: `{ tag }`
- Returns: Tag ID

### Remove Tag by ID
- **DELETE** `/schemas/tags/{tagId}`
- Removes a tag by its ID
- Returns: Success status

### Remove Tag by Value
- **DELETE** `/schemas/{id}/tags?tag={tagValue}`
- Removes a tag by schema ID and tag value
- Returns: Success status

## Validation Specification Endpoints

### Get Validation Spec
- **GET** `/validationspecs/{id}`
- Gets validation specification details
- Returns: Validation spec with rules and references

### Get Validation Spec by Schema
- **GET** `/validationspecs/by-schema/{schemaId}`
- Gets validation specification for a schema
- Returns: Validation spec or 404 if not found

### Validate Validation Spec
- **POST** `/validationspecs/{id}/validate`
- Validates a validation specification
- Returns: Validation result with errors and warnings

### Create Validation Spec
- **POST** `/validationspecs`
- Creates a new validation specification
- Request body: `{ dataSchemaId }`
- Returns: Validation spec ID

### Add Validation Rule
- **POST** `/validationspecs/{id}/rules`
- Adds a validation rule
- Request body: `{ ruleType, severity, parameters }`
- Returns: Rule ID

### Update Validation Rule
- **PUT** `/validationspecs/rules/{ruleId}`
- Updates a validation rule
- Request body: `{ ruleType?, severity?, parameters? }`
- Returns: Success status

### Remove Validation Rule
- **DELETE** `/validationspecs/rules/{ruleId}`
- Removes a validation rule
- Returns: Success status

### Add Validation Reference
- **POST** `/validationspecs/{id}/references`
- Adds a validation reference (child validation spec)
- Request body: `{ fieldPath, childValidationSpecId }`
- Returns: Reference ID

### Publish Validation Spec
- **POST** `/validationspecs/{id}/publish`
- Publishes a validation specification
- Request body: `{ publishedBy }`
- Returns: Success status

## Transformation Specification Endpoints

### Get Transformation Spec
- **GET** `/transformationspecs/{id}`
- Gets transformation specification details
- Returns: Transformation spec with rules, graph, bindings, references

### Get Transformation Spec by Source Schema
- **GET** `/transformationspecs/by-source-schema/{sourceSchemaId}`
- Gets transformation specification for a source schema
- Returns: Transformation spec or 404 if not found

### Get Compiled Transformation Spec
- **GET** `/transformationspecs/{id}/compiled`
- Gets the compiled (execution-ready) representation of a published transformation spec
- Returns: Compiled transformation structure

### Get Compatible Transformation Specs
- **GET** `/transformationspecs/compatible?sourceSchemaId={id}&targetSchemaId={id}&status={status?}`
- Gets transformation specs compatible with source and target schemas
- Used for discovering nested transformation options for Object/Object-Array fields
- **For Object/Object-Array fields**: Queries by ElementSchemaId
- **For Scalar Arrays**: Uses virtual scalar element schemas internally (not exposed via API)
- Optional status filter (defaults to all statuses, returns both Published and Draft)
- Returns: Array of compatible transformation spec summaries with schema details

### Validate Transformation Spec
- **POST** `/transformationspecs/{id}/validate`
- Validates a transformation specification
- Returns: Validation result with errors and warnings

### Create Transformation Spec
- **POST** `/transformationspecs`
- Creates a new transformation specification
- Request body: `{ sourceSchemaId, targetSchemaId, mode, type }`
- Returns: Transformation spec ID

### Add Simple Transform Rule
- **POST** `/transformationspecs/{id}/simple-rules`
- Adds a simple transformation rule
- Request body: `{ sourcePath, targetPath, converterId?, required, order }`
- Returns: Rule ID

### Update Simple Transform Rule
- **PUT** `/transformationspecs/simple-rules/{ruleId}`
- Updates a simple transformation rule
- Request body: `{ sourcePath?, targetPath?, converterId?, required?, order? }`
- Returns: Success status

### Remove Simple Transform Rule
- **DELETE** `/transformationspecs/simple-rules/{ruleId}`
- Removes a simple transformation rule
- Returns: Success status

### Add Transformation Reference
- **POST** `/transformationspecs/{id}/references`
- Adds a transformation reference (child transformation spec)
- Request body: `{ sourceFieldPath, targetFieldPath, childTransformationSpecId }`
- **Requirements**:
  - Source and target fields must be Object or Array type
  - For Object/Object-Array: Child transformation source/target schemas must match ElementSchemaId
  - For Scalar Arrays: Child transformation source/target schemas must match virtual scalar element schemas
  - Child transformation must be Published (or Draft if parent is also Draft)
  - For Array → Array: Child transformation must have OneToOne cardinality (applies per-element)
- Returns: Reference ID

### Publish Transformation Spec
- **POST** `/transformationspecs/{id}/publish`
- Publishes a transformation specification
- Request body: `{ publishedBy }`
- Returns: Success status

### Add Graph Node (Advanced Mode)
- **POST** `/transformationspecs/{id}/graph-nodes`
- Adds a node to the transformation graph
- Request body: `{ nodeType, key, config? }`
- Returns: Node ID

### Remove Graph Node (Advanced Mode)
- **DELETE** `/transformationspecs/graph-nodes/{nodeId}`
- Removes a node from the transformation graph
- Returns: Success status

### Add Graph Edge (Advanced Mode)
- **POST** `/transformationspecs/{id}/graph-edges`
- Adds an edge to the transformation graph
- Request body: `{ fromNodeId, toNodeId, fromOutputKey, toInputKey }`
- Returns: Edge ID

### Remove Graph Edge (Advanced Mode)
- **DELETE** `/transformationspecs/graph-edges/{edgeId}`
- Removes an edge from the transformation graph
- Returns: Success status

### Add Output Binding (Advanced Mode)
- **POST** `/transformationspecs/{id}/output-bindings`
- Adds an output binding (maps graph output to target field)
- Request body: `{ fromNodeId, fromOutputKey, targetPath }`
- Returns: Binding ID

## Business Key Endpoints

### Add Business Key
- **POST** `/keydefinitions`
- Adds a business key definition to a schema
- Request body: `{ dataSchemaId, name, isPrimary }`
- Returns: Key definition ID

### Remove Business Key
- **DELETE** `/keydefinitions/{id}`
- Removes a business key definition
- Returns: Success status

### Add Key Field
- **POST** `/keydefinitions/{id}/fields`
- Adds a field to a business key
- Request body: `{ fieldPath, order, normalization? }`
- Returns: Key field ID

### Remove Key Field
- **DELETE** `/keydefinitions/fields/{fieldId}`
- Removes a field from a business key
- Returns: Success status

### Reorder Key Fields
- **PUT** `/keydefinitions/{id}/fields/reorder`
- Reorders fields in a business key
- Request body: `{ fieldIds[] }` (ordered list)
- Returns: Success status

### Get Schema Business Keys
- **GET** `/keydefinitions/schemas/{schemaId}`
- Gets all business keys for a schema
- Returns: Array of key definitions with fields

## Response Formats

### Success Responses

- **IdResponse**: Contains an ID (string UUID)
- **SuccessResponse**: Contains a success boolean

### Error Responses

- **400 Bad Request**: Invalid input parameters
- **404 Not Found**: Resource not found
- **409 Conflict**: Business rule violation (e.g., duplicate field path, schema referenced)
- **500 Internal Server Error**: Unexpected server error

### Validation Responses

- **ValidationResult**: Contains isValid boolean, errors array, warnings array
- Each error has a field path and message

## Authentication & Authorization

Currently, the service does not implement authentication/authorization. This is planned for future implementation.

## Rate Limiting

Currently, the service does not implement rate limiting. This is planned for future implementation.


