# Core Concepts

This document explains the fundamental concepts and UI patterns in the Loom Data Studio UI.

## Navigation Structure

The application follows a 3-level navigation hierarchy:

1. **Data Studio** → Role Selection (Incoming, Master, Outgoing)
2. **Schema Overview** → List of schemas for the selected role
3. **Schema Key Overview** → Version history for a specific schema key
4. **Schema Version Detail** → Detailed editor for a specific schema version

### Navigation Paths

- `/data-studio/incoming` - Incoming data schemas overview
- `/data-studio/incoming/{schemaKey}` - Schema key overview
- `/data-studio/incoming/{schemaKey}/{versionId}` - Schema version detail editor
- `/data-studio/master` - Master data schemas overview
- `/data-studio/master/{schemaKey}` - Schema key overview
- `/data-studio/master/{schemaKey}/{versionId}` - Schema version detail editor
- `/data-studio/outgoing` - Outgoing data schemas overview
- `/data-studio/outgoing/{schemaKey}` - Schema key overview
- `/data-studio/outgoing/{schemaKey}/{versionId}` - Schema version detail editor

## Schema Overview Page

The schema overview page displays all schemas for a selected role.

### Features

- **Schema Cards**: Grid layout showing schema key, status, version, description, tags
- **Filtering**: Filter by status (Draft, Published, Archived, All)
- **Search**: Search by schema key or description
- **Create Schema**: Button to create new schema
- **Delete Schema**: Delete button (red X) on each card
- **Navigation**: Click schema card to view version history

### Schema Card Information

- Schema key (clickable link)
- Status badge (Draft/Published/Archived)
- Latest version number
- Published date (if published)
- Tags (if any)
- Delete button (red X icon)

## Schema Key Overview Page

The schema key overview page shows the version history for a specific schema.

### Features

- **Version Table**: List of all versions (newest to oldest)
- **Version Information**: Version number, status, created date, published date
- **Actions**: Open version, create new draft, delete latest version
- **Tags Section**: Manage tags for the latest version
  - View all tags
  - Add new tags
  - Remove tags (red X on each tag)

### Version Actions

- **Open Version**: Link to open any version in the detail editor
- **Create New Draft**: Button to create new draft from latest published version
- **Delete Version**: Red X button (only for latest version)

## Schema Version Detail Page

The schema version detail page is the main editor for configuring a schema version.

### Layout

The page uses a consistent 3-panel layout:

- **Left Panel**: List of items (fields, rules, transformations, keys)
- **Center Panel**: Details/editor for selected item
- **Right Panel**: Removed (details now in center panel)

### Tabs

The page has multiple tabs for different aspects of schema configuration:

1. **Structure**: Schema field definitions
2. **Validations**: Validation rules
3. **Transformations**: Transformation specifications
4. **Keys**: Business key definitions (Master schemas only)
5. **References**: Schema dependency graph (Expert mode only)

### Expert Mode

- **Toggle**: Checkbox in header to enable/disable expert mode
- **Beginner Mode (Default)**:
  - Simple field structure editing
  - Basic validation rules
  - Simple transformation mode only
  - Basic business key editing

- **Expert Mode**:
  - Advanced validation rules
  - Advanced transformation graph editor
  - Multiple business keys with normalization
  - Schema reference visualization
  - Dependency graph viewer

## Structure Editor

The structure editor manages schema field definitions.

### Layout

- **Left Panel**: Tree view of all fields
- **Center Panel**: Field details editor (when field selected)

### Features

- **Add Field**: Button to add new field
- **Field Types**: Scalar, Object, Array
- **Field Configuration**:
  - Path (unique identifier)
  - Field type selection
  - **For Scalar fields**: Scalar type dropdown (String, Integer, Decimal, Boolean, Date, DateTime, Time, Guid)
  - **For Object fields**: Schema reference autocomplete dropdown
  - **For Array fields**: Element type selector:
    - **Scalar value**: Select scalar type (e.g., String, Integer) → creates scalar array (e.g., `string[]`, `int[]`)
    - **Object**: Select schema reference → creates object array (e.g., `OrderItem[]`)
  - Required toggle
  - Description
- **Field Actions**:
  - Select field to view/edit details
  - Delete field (red X icon in list)
  - Drag & drop to reorder (future)

### Field Reference Selection

- Autocomplete dropdown showing available schemas
- Filters by role (same role as current schema)
- Shows both Draft and Published schemas when editing
- Only Published schemas allowed when publishing

## Validation Editor

The validation editor manages validation rules for a schema.

### Layout

- **Left Panel**: List of validation rules
- **Center Panel**: Rule details editor (when rule selected)

### Features

- **Create Validation Spec**: Button if no spec exists
- **Add Rule**: Button to add new validation rule
- **Rule Types**: Field, CrossField, Conditional
- **Rule Configuration**:
  - Rule type selection
  - Severity (Error or Warning)
  - Parameters (JSON or field-specific UI)
- **Rule Actions**:
  - Select rule to view/edit details
  - Delete rule (red X icon in list)
  - Edit rule properties

### Rule Editor

- Field path picker (for Field rules)
- Conditional builder UI (for Conditional rules)
- JSON editor (for complex rules)
- Backend validation errors shown inline

## Transformation Editor

The transformation editor manages data transformation specifications.

### Modes

1. **Simple Mode (Default)**:
   - Table-based field mapping
   - Source field → Target field
   - Optional converter
   - Required toggle

2. **Advanced Mode (Expert Mode Only)**:
   - Visual graph-based editor
   - Node types: Source, Map, Filter, Aggregate, Join, Split, Constant, Expression
   - Typed inputs/outputs
   - Acyclic graph validation
   - Output bindings to target schema

### Layout

- **Header**: Title and mode switch buttons (above both panels)
- **Left Panel**: List of transformation rules (Simple) or graph nodes (Advanced)
- **Center Panel**: Rule/node details editor

### Simple Mode Features

- **Create Transformation Spec**: Button to create spec and select target schema
- **Add Rule**: Button to add field mapping rule
- **Rule Configuration**:
  - Source field picker (shows field type in dropdown)
  - Target field picker (shows field type in dropdown)
  - **Type Compatibility Validation**:
    - Shows warnings for incompatible mappings (e.g., Object → Scalar)
    - Blocks invalid mappings in Simple mode with helpful error messages
    - Guides users to Advanced mode for structure-changing transformations
  - **Nested Transformation Selector** (for Object/Object-Array fields):
    - Automatically appears when mapping Object or Object-Array fields
    - Queries backend for compatible transformation specs (Published and Draft)
    - Auto-suggests if exactly one match exists
    - Displays current nested transformation if configured
    - Allows selection or change of nested transformation
    - Shows warning if no transformation is defined
    - **Note**: Scalar arrays (`scalar[]`) do not require nested transformations for direct mappings
  - Converter ID (optional)
  - Required toggle
- **Rule Actions**:
  - Select rule to view/edit details
  - Delete rule (red X icon in list)

### Advanced Mode Features

- **Graph Editor**: React Flow-based visual editor
- **Node Types**: Various transformation node types
- **Node Configuration**: Type-specific configuration panels
- **Edge Creation**: Connect nodes with typed inputs/outputs
- **Output Bindings**: Map graph outputs to target schema fields
- **Graph Validation**: Acyclic check, type compatibility

### Mode Switching

- **Warning Modal**: Appears when switching from Simple to Advanced
- **One-Way Upgrade**: Advanced mode cannot be downgraded to Simple
- **Confirmation Required**: User must confirm the upgrade

## Business Keys Editor

The business keys editor manages business key definitions (Master schemas only).

### Layout

- **Full Width**: Single panel layout
- **Key List**: List of all business keys
- **Key Editor**: Editor for selected key

### Features

- **Add Key**: Button to add new business key
- **Key Configuration**:
  - Key name
  - Primary flag (exactly one primary key)
  - Field selection (scalar fields only)
  - Field ordering (drag & drop)
  - Normalization (Expert mode only)
- **Key Actions**:
  - Select key to edit
  - Mark as primary
  - Remove key (Draft only)

### Field Selection

- Field picker shows only scalar fields
- For primary keys: Only required fields shown
- Drag & drop ordering
- Normalization dropdown (Expert mode)

## Publishing Flow

### Publishing Process

1. **Click Publish Button**
   - Button in header (green, or red if errors exist)

2. **Backend Validation**
   - System runs comprehensive validation
   - Checks schema structure, validations, transformations, keys
   - Checks for unpublished dependencies

3. **Error Handling**
   - If errors exist: Errors displayed inline
   - If unpublished dependencies: Modal appears with list
   - User can choose to bulk publish dependencies

4. **Publishing**
   - If validation passes: Schema is published
   - Status changes to Published
   - Schema becomes read-only

### Validation Feedback

- **Inline Errors**: Errors shown in relevant tabs
- **Error Highlighting**: Offending fields/rules/nodes highlighted
- **Error Messages**: Field-addressable error messages
- **Dependency Modal**: Lists unpublished schemas that need publishing

### Published Schema Behavior

- **Read-Only**: All editing controls disabled
- **Visual Indication**: Clear status badge and UI indicators
- **View-Only**: Can view all configuration but cannot modify

## Tag Management

Tags are managed on the Schema Key Overview page.

### Features

- **Tag Section**: Dedicated section showing all tags for latest version
- **Add Tag**: Button to add new tag
- **Tag Input**: Text input with Enter/Escape key support
- **Tag Display**: Tags shown as styled badges
- **Remove Tag**: Red X button on each tag
- **Error Handling**: Inline error messages for failed operations

### Tag UI

- Tags displayed as colored badges
- Add button opens input field
- Enter key to add, Escape to cancel
- Delete button (red X) on each tag
- Real-time updates after add/remove

## Delete Functionality

### Delete Schema Version

- **Location**: Schema Version Detail page header
- **Button**: Red border button "Delete Version"
- **Visibility**: Only shown for latest version
- **Confirmation**: Modal dialog with confirmation
- **Behavior**: Deletes version and navigates back to key overview

### Delete Entire Schema

- **Location**: Schema Overview page (schema cards)
- **Button**: Red X icon on each card
- **Confirmation**: Modal dialog warning about deleting all versions
- **Validation**: Cannot delete if schema is referenced by other schemas
- **Error**: Error message lists all referencing schemas

### Delete Confirmation Dialogs

- **Version Delete**: Shows version number and schema key
- **Schema Delete**: Warns about deleting all versions
- **Error Display**: Shows backend error messages if deletion fails


