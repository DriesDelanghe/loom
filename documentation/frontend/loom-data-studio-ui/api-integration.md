# API Integration

This document describes how the frontend integrates with the Master Data Configuration Service API.

## API Client

The frontend uses a centralized API client located in `src/api/masterdata.ts`.

### Base Configuration

- **Base URL**: Configured via `VITE_API_BASE_URL` environment variable
- **Default**: `/api` (relative path for same-origin requests)
- **Content-Type**: `application/json` for all requests
- **Error Handling**: Centralized error handling with custom `ApiError` class

### API Methods

The API client is organized into logical groups:

#### Data Models API

- `createDataModel(key, name, description?)` - Create a new data model

#### Schemas API

- `getSchemas(role?, status?)` - Get list of schemas
- `getSchemaDetails(schemaId)` - Get schema details
- `getSchemaGraph(schemaId)` - Get schema reference graph
- `createSchema(dataModelId?, role, key, description?)` - Create new schema
- `addField(schemaId, path, fieldType, scalarType?, elementSchemaId?, required, description?)` - Add field
- `updateField(fieldId, path?, fieldType?, scalarType?, elementSchemaId?, required?, description?)` - Update field
- `removeField(fieldId)` - Remove field
- `publishSchema(schemaId, publishedBy)` - Publish schema
- `validateSchema(schemaId)` - Validate schema
- `getUnpublishedDependencies(schemaId)` - Get unpublished dependencies
- `publishRelatedSchemas(schemaId, publishedBy, relatedSchemaIds[])` - Bulk publish dependencies
- `deleteSchemaVersion(schemaVersionId)` - Delete schema version
- `deleteSchema(key, role)` - Delete entire schema
- `addSchemaTag(schemaId, tag)` - Add tag
- `removeSchemaTagByValue(schemaId, tag)` - Remove tag

#### Validation Specs API

- `getValidationSpec(id)` - Get validation spec
- `getValidationSpecBySchemaId(schemaId)` - Get validation spec for schema
- `createValidationSpec(schemaId)` - Create validation spec
- `addValidationRule(specId, ruleType, severity, parameters)` - Add rule
- `updateValidationRule(ruleId, ruleType?, severity?, parameters?)` - Update rule
- `removeValidationRule(ruleId)` - Remove rule
- `addValidationReference(specId, fieldPath, childSpecId)` - Add reference
- `publishValidationSpec(specId, publishedBy)` - Publish spec
- `validateValidationSpec(specId)` - Validate spec

#### Transformation Specs API

- `getTransformationSpec(id)` - Get transformation spec
- `getTransformationSpecBySourceSchemaId(sourceSchemaId)` - Get transformation spec for source schema
- `createTransformationSpec(sourceSchemaId, targetSchemaId, mode, type)` - Create transformation spec
- `addSimpleTransformRule(specId, sourcePath, targetPath, converterId?, required, order)` - Add simple rule
- `updateSimpleTransformRule(ruleId, sourcePath?, targetPath?, converterId?, required?, order?)` - Update simple rule
- `removeSimpleTransformRule(ruleId)` - Remove simple rule
- `addTransformReference(specId, sourceFieldPath, targetFieldPath, childSpecId)` - Add nested transformation reference
- `getCompatibleTransformationSpecs(sourceSchemaId, targetSchemaId, status?)` - Get compatible transformations for nested mapping
- `publishTransformationSpec(specId, publishedBy)` - Publish spec
- `validateTransformationSpec(specId)` - Validate spec
- `getCompiledTransformationSpec(specId)` - Get compiled spec

#### Key Definitions API

- `addBusinessKey(schemaId, name, isPrimary)` - Add business key
- `removeBusinessKey(keyId)` - Remove business key
- `addKeyField(keyId, fieldPath, order, normalization?)` - Add key field
- `removeKeyField(fieldId)` - Remove key field
- `reorderKeyFields(keyId, fieldIds[])` - Reorder key fields
- `getSchemaBusinessKeys(schemaId)` - Get all keys for schema

## React Query Integration

The frontend uses React Query (TanStack Query) for all API calls.

### Query Keys

Query keys follow a consistent pattern:

- `['schemas', role, status?]` - Schema list queries
- `['schemaDetails', versionId]` - Schema detail queries
- `['schemaGraph', schemaId]` - Schema graph queries
- `['validateSchema', schemaId]` - Validation result queries
- `['unpublishedDependencies', schemaId]` - Dependency queries
- `['validationSpec', schemaId]` - Validation spec queries
- `['transformationSpec', schemaId]` - Transformation spec queries

### Mutations

Mutations use `useMutation` hook with:

- **mutationFn**: API call function
- **onSuccess**: Cache invalidation and UI updates
- **onError**: Error handling (optional)

### Cache Invalidation

After mutations, relevant queries are invalidated:

```typescript
queryClient.invalidateQueries({ queryKey: ['schemas', role] })
queryClient.invalidateQueries({ queryKey: ['schemaDetails', versionId] })
```

This ensures UI stays in sync with backend state.

## Error Handling

### API Errors

- **Network Errors**: Handled by React Query
- **HTTP Errors**: Parsed and displayed to user
- **Validation Errors**: Shown inline in relevant UI components
- **Business Rule Errors**: Displayed with descriptive messages

### Error Display

- **Inline Errors**: Shown in form fields or panels
- **Modal Errors**: For critical operations (publish, delete)
- **Toast Notifications**: For success/error feedback (future)

## Data Flow

### Typical Data Flow

1. **User Action** → Component event handler
2. **Mutation Call** → `useMutation` hook
3. **API Request** → API client method
4. **Backend Processing** → Master Data Configuration Service
5. **Response** → Success or error
6. **Cache Update** → React Query cache invalidation
7. **UI Update** → Component re-renders with new data

### Optimistic Updates

- Some operations use optimistic updates
- UI updates immediately
- Rollback on error
- Example: Adding/removing tags

## Type Safety

### TypeScript Types

All API responses are strongly typed:

- `DataSchemaSummary` - Schema list item
- `DataSchemaDetails` - Complete schema details
- `FieldDefinitionSummary` - Field information
- `ValidationSpecDetails` - Validation specification
- `TransformationSpecDetails` - Transformation specification
- `KeyDefinitionSummary` - Business key information

### Type Alignment

- Frontend types match backend DTOs exactly
- Enum values align between frontend and backend
- Type mismatches cause compile-time errors

## Authentication

Currently, the frontend does not implement authentication. This is planned for future implementation.

## Real-time Updates

- **Polling**: Validation results poll every 30 seconds for Draft schemas
- **Manual Refresh**: User can manually refresh data
- **Cache Invalidation**: Automatic cache updates after mutations


