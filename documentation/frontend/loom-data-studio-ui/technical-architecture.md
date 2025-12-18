# Technical Architecture

This document describes the technical architecture and implementation details of the Data Studio UI.

## Project Structure

```
src/
├── api/              # API client modules
│   ├── client.ts    # Base API client
│   └── masterdata.ts # Master data API methods
├── components/       # Reusable UI components
│   └── editor/      # Schema editor components
│       ├── SchemaStructureEditor.tsx
│       ├── InspectorPanel.tsx
│       ├── ValidationEditor.tsx
│       ├── ValidationRuleInspector.tsx
│       ├── TransformationEditor.tsx
│       ├── SimpleTransformationEditor.tsx
│       ├── TransformationRuleInspector.tsx
│       ├── KeyDefinitionsEditor.tsx
│       └── SchemaReferenceGraph.tsx
├── pages/           # Route-level components
│   ├── SchemaOverviewPage.tsx
│   ├── SchemaKeyOverviewPage.tsx
│   └── SchemaVersionDetailPage.tsx
├── types/           # TypeScript type definitions
│   └── index.ts
├── utils/           # Utility functions
│   └── nodeMetadata.ts
└── App.tsx          # Root component with routing
```

## Component Architecture

### Page Components

Page components are top-level route handlers:

- **SchemaOverviewPage**: Lists schemas for a role
- **SchemaKeyOverviewPage**: Shows version history and tags
- **SchemaVersionDetailPage**: Main schema editor

### Editor Components

Editor components handle specific aspects of schema configuration:

- **SchemaStructureEditor**: Tree-based field structure editor
- **ValidationEditor**: Validation rule list and management
- **TransformationEditor**: Wrapper for simple/advanced transformation editors
- **KeyDefinitionsEditor**: Business key editor (Master schemas)

### Inspector Components

Inspector components show details for selected items:

- **InspectorPanel**: Field details editor
- **ValidationRuleInspector**: Validation rule details editor
- **TransformationRuleInspector**: Transformation rule details editor

### Graph Components

- **SchemaReferenceGraph**: Visual dependency graph viewer (React Flow)

## State Management

### Server State (React Query)

- All API data managed by React Query
- Automatic caching and invalidation
- Optimistic updates for mutations
- Background refetching for validation results

### Local State (React Hooks)

- UI state (modals, tabs, selections) managed with `useState`
- Form state managed locally in components
- Computed values with `useMemo`
- Side effects with `useEffect`

## Layout Patterns

### Three-Panel Layout

The schema version detail page uses a consistent 3-panel layout:

- **Left Panel (w-64)**: List of items (fields, rules, transformations)
- **Center Panel (flex-1)**: Details/editor for selected item
- **Right Panel**: Removed (details now in center)

### Responsive Design

- Fixed-width left panel (256px)
- Flexible center panel
- Overflow handling with scrollable containers

## Form Handling

### Controlled Components

All form inputs use controlled components:

```typescript
const [value, setValue] = useState(initialValue)
<input value={value} onChange={(e) => setValue(e.target.value)} />
```

### Form Validation

- Client-side validation for immediate feedback
- Backend validation as source of truth
- Error messages displayed inline
- Field-addressable errors from backend

## Editor Patterns

### List-Detail Pattern

- Left panel: List of items
- Center panel: Details for selected item
- Selection state managed with `useState`
- Empty state when no item selected

### Edit Mode Pattern

- View mode: Display values
- Edit mode: Editable form
- "Edit" button toggles mode
- "Save" and "Cancel" buttons in edit mode
- Changes saved via mutation

### Delete Pattern

- Red X icon in list items
- Confirmation dialogs for destructive actions
- Error handling with user-friendly messages

## API Integration Patterns

### Query Pattern

```typescript
const { data, isLoading, error } = useQuery({
  queryKey: ['schemaDetails', versionId],
  queryFn: () => schemasApi.getSchemaDetails(versionId!),
  enabled: !!versionId,
})
```

### Mutation Pattern

```typescript
const mutation = useMutation({
  mutationFn: (data) => api.method(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['schemas'] })
    // Reset form state
  },
})
```

## Type Safety

### Type Definitions

All types defined in `src/types/index.ts`:

- Schema types (DataSchemaSummary, DataSchemaDetails)
- Field types (FieldDefinitionSummary)
- Validation types (ValidationSpecDetails, RuleType, Severity)
- Transformation types (TransformationSpecDetails, TransformationMode)
- Key types (KeyDefinitionSummary)
- Enum types (SchemaRole, SchemaStatus, FieldType, ScalarType)

### Type Imports

- Use `type` keyword for type-only imports
- Import types from same file when exporting both component and types

## Styling

### Tailwind CSS

- Utility-first CSS framework
- Consistent color scheme (loom-600, loom-700, etc.)
- Responsive utilities
- Conditional classes with template literals

### Component Styling

- Inline styles for dynamic values (positions, colors)
- Tailwind classes for layout and appearance
- Consistent spacing and sizing

## Performance Optimizations

### Memoization

- `memo()` for React Flow nodes
- `useCallback` for event handlers
- `useMemo` for computed values

### Code Splitting

- Route-based code splitting (future)
- Dynamic imports for heavy components (future)

## Error Boundaries

Currently, error boundaries are not implemented. This is planned for future implementation.

## Accessibility

- Semantic HTML elements
- Keyboard navigation support
- ARIA labels where needed
- Focus management for modals

## Browser Support

- Modern browsers (Chrome, Firefox, Safari, Edge)
- ES2020+ features
- CSS Grid and Flexbox


