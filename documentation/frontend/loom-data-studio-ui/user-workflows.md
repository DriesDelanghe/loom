# User Workflows

This document describes the key user workflows and interactions in the Data Studio UI.

## Creating a New Schema

### Workflow

1. **Navigate to Schema Overview**
   - User opens the application
   - Selects role (Incoming, Master, or Outgoing)
   - Lands on schema overview page (e.g., `/data-studio/master`)

2. **Create Schema**
   - Click "Create Schema" button
   - Enter schema key (required, unique within role)
   - Enter optional description
   - Click "Create"
   - System creates schema with version 1 in Draft status
   - User is redirected to schema version detail editor

3. **Define Schema Structure**
   - Navigate to Structure tab
   - Click "Add Field" button
   - Configure field:
     - Enter field path (e.g., "customer.email")
     - Select field type (Scalar, Object, Array)
     - If Scalar: Select scalar type
     - If Object/Array: Select referenced schema from dropdown
     - Configure required flag
     - Add optional description
   - Repeat for all fields

4. **Configure Validations (Optional)**
   - Navigate to Validations tab
   - Click "Create Validation Spec" if none exists
   - Click "Add Rule" button
   - Configure rule:
     - Select rule type (Field, CrossField, Conditional)
     - Select severity (Error or Warning)
     - Configure parameters (field path picker or JSON)
   - Repeat for all rules

5. **Configure Transformations (Optional)**
   - Navigate to Transformations tab
   - Click "Create Transformation Spec" if none exists
   - Select target schema
   - For Simple mode: Add field mapping rules
   - For Advanced mode (Expert): Build transformation graph

6. **Define Business Keys (Master Schemas Only)**
   - Navigate to Keys tab
   - Click "Add Key" button
   - Configure key:
     - Enter key name
     - Mark as primary (if no primary exists)
     - Add fields with ordering
     - Configure normalization (Expert mode)

7. **Validate and Publish**
   - Click "Publish" button in header
   - System validates schema
   - If errors: Review and fix errors
   - If unpublished dependencies: Use bulk publish modal
   - Click "Publish" to make schema usable

## Editing an Existing Schema

### Workflow

1. **Navigate to Schema**
   - From schema overview, click on schema card
   - Navigate to schema key overview page

2. **Open Draft Version**
   - If draft exists, click "Open Draft" button
   - If no draft, click "Create New Draft" from latest published version
   - Navigate to schema version detail editor

3. **Modify Schema**
   - Edit fields, validations, transformations, keys as needed
   - Changes are saved automatically as user works
   - Use tabs to navigate between different aspects

4. **Review Changes**
   - Check validation status (button color indicates errors)
   - Review validation errors if any
   - Fix any blocking errors

5. **Publish New Version**
   - Click "Publish" button
   - Validate and publish the new draft
   - New version becomes Published
   - Previous published versions remain Published

## Working with Fields

### Adding a Field

1. **Navigate to Structure Tab**
   - Click on Structure tab in schema version detail page

2. **Click "Add Field" Button**
   - Button in left panel
   - Opens field creation form

3. **Configure Field**
   - Enter field path (unique identifier)
   - Select field type (Scalar, Object, Array)
   - **If Scalar**: Select scalar type from dropdown (String, Integer, Decimal, Boolean, Date, DateTime, Time, Guid)
   - **If Object**: Select referenced schema from autocomplete dropdown
   - **If Array**: Select element type:
     - **Scalar value**: Select scalar type (e.g., String, Integer) → creates scalar array (e.g., `string[]`, `int[]`)
     - **Object**: Select referenced schema from autocomplete dropdown → creates object array (e.g., `OrderItem[]`)
   - Toggle required flag
   - Add optional description

4. **Field Appears in Tree**
   - Field is added to structure tree
   - Can be selected to view/edit details

### Editing a Field

1. **Select Field**
   - Click on field in structure tree
   - Field details appear in center panel

2. **Click "Edit" Button**
   - Button in center panel header
   - Form becomes editable

3. **Modify Properties**
   - Update path, type, scalar type, reference, required, description
   - **When switching field types**: UI automatically clears incompatible values (e.g., switching from Scalar to Object clears ScalarType)
   - **For Array fields**: Can switch between scalar array and object array (element type selector)
   - Click "Save" to apply changes
   - Click "Cancel" to discard changes

### Removing a Field

1. **Click Delete Button**
   - Red X icon next to field in structure tree
   - Confirmation may be required

2. **Field Removed**
   - Field is removed from schema
   - Validation errors may appear if field was referenced

## Working with Validations

### Creating a Validation Spec

1. **Navigate to Validations Tab**
   - Click on Validations tab

2. **Create Validation Spec**
   - If no spec exists, click "Create Validation Spec" button
   - System creates empty validation specification

3. **Add Validation Rules**
   - Click "Add Rule" button
   - Configure rule type, severity, parameters
   - Rules appear in left panel list

### Adding a Validation Rule

1. **Click "Add Rule" Button**
   - Button in left panel header
   - Form appears below

2. **Configure Rule**
   - Select rule type (Field, CrossField, Conditional)
   - Select severity (Error or Warning)
   - For Field rules: Enter field path
   - For complex rules: Configure JSON parameters

3. **Rule Added**
   - Rule appears in left panel list
   - Can be selected to view/edit details

### Editing a Validation Rule

1. **Select Rule**
   - Click on rule in left panel
   - Rule details appear in center panel

2. **Click "Edit" Button**
   - Form becomes editable

3. **Modify Properties**
   - Update rule type, severity, parameters
   - Click "Save" to apply changes

### Removing a Validation Rule

1. **Click Delete Button**
   - Red X icon next to rule in left panel list
   - Rule is removed immediately

## Working with Transformations

### Creating a Simple Transformation

1. **Navigate to Transformations Tab**
   - Click on Transformations tab

2. **Create Transformation Spec**
   - If no spec exists, click "Create Transformation Spec"
   - Select target schema from dropdown
   - System creates transformation specification in Simple mode

3. **Add Mapping Rules**
   - Click "Add Rule" button
   - Select source field from dropdown (shows field type, e.g., "email (Scalar)", "customer (Object)", "tags (string[])")
   - Select target field from dropdown (shows field type)
   - **Type Compatibility Validation**:
     - UI validates field type compatibility
     - Shows warnings for incompatible mappings (e.g., Object → Scalar)
     - **Allowed in Simple Mode**:
       - `scalar → scalar`: Direct mapping
       - `scalar[] → scalar[]`: Element-wise copy (same scalar type)
       - `object[] → scalar[]`: Field extraction
       - `object[] → object[]`: Same schema (requires TransformReference)
     - **Blocked in Simple Mode** (shows error with "Open Advanced Editor" button):
       - `scalar[] → object[]`: Structure-changing transformation
       - `object[] → object[]`: Different schemas without TransformReference
   - If fields are Object or Object-Array type:
     - Nested transformation selector appears
     - System queries for compatible transformations (Published and Draft)
     - If exactly one match: UI suggests it (user must confirm)
     - If multiple matches: User must select one
     - If no matches: User must create transformation first
     - Select or create nested transformation
   - **Note**: Scalar arrays (`scalar[]`) do not require nested transformations for direct mappings
   - Optionally specify converter ID
   - Configure required flag
   - Rule appears in left panel list

### Switching to Advanced Mode

1. **Enable Expert Mode**
   - Check "Expert Mode" checkbox in header

2. **Switch to Advanced Mode**
   - Click "Advanced Mode" button in transformation header
   - Warning modal appears

3. **Confirm Upgrade**
   - Read warning about one-way upgrade
   - Click "Confirm" to proceed
   - Transformation switches to Advanced mode

4. **Build Transformation Graph**
   - Add graph nodes (Source, Map, Filter, etc.)
   - Connect nodes with edges
   - Configure output bindings
   - Graph is validated for acyclicity

### Editing Transformation Rules

1. **Select Rule**
   - Click on rule in left panel
   - Rule details appear in center panel

2. **Edit Rule**
   - Click "Edit" button
   - Modify source/target fields, converter, required flag
   - If fields are Object or Array: Configure nested transformation
   - Click "Save" to apply changes

### Configuring Nested Transformations

1. **When Mapping Object/Object-Array Fields**
   - Nested transformation selector appears automatically
   - System queries backend for compatible transformations (Published and Draft)
   - **Note**: Scalar arrays (`scalar[]`) do not require nested transformations for direct mappings

2. **Auto-Suggestion**
   - If exactly one compatible transformation exists: UI suggests it
   - User must confirm the suggestion
   - If multiple exist: User must select one
   - If none exist: User must create transformation first
   - Draft transformations are shown with visual indicators (yellow background, warning badge)

3. **Select Transformation**
   - View available transformations with schema names and versions
   - Expert mode shows: IDs, versions, cardinality, status (Draft/Published)
   - Beginner mode shows: Schema names and version numbers
   - Select transformation and confirm
   - **Warning**: Draft transformations can only be used when parent transformation is also Draft

4. **View Current Transformation**
   - Current nested transformation displayed if configured
   - Shows transformation details (schema names, version, status)
   - Can change transformation if needed
   - Warning shown if no transformation is defined (for Object/Object-Array mappings)

### Removing Transformation Rules

1. **Click Delete Button**
   - Red X icon next to rule in left panel list
   - Rule is removed immediately
   - Associated nested transformation reference is also removed

## Working with Business Keys

### Adding a Business Key

1. **Navigate to Keys Tab**
   - Click on Keys tab (Master schemas only)

2. **Click "Add Key" Button**
   - Button to add new business key

3. **Configure Key**
   - Enter key name
   - Mark as primary (if no primary exists)
   - Add fields to key:
     - Select fields from picker (scalar, required for primary)
     - Drag & drop to order fields
     - Configure normalization (Expert mode)

4. **Key Added**
   - Key appears in key list
   - Can be selected to edit

### Editing Business Keys

1. **Select Key**
   - Click on key in key list
   - Key details appear in editor

2. **Modify Key**
   - Update key name
   - Change primary flag
   - Add/remove/reorder fields
   - Update normalization

### Removing Business Keys

1. **Click Delete Button**
   - Delete button for key
   - Key is removed (Draft only)

## Managing Tags

### Adding Tags

1. **Navigate to Schema Key Overview**
   - Go to schema key overview page

2. **Click "Add Tag" Button**
   - Button in Tags section
   - Input field appears

3. **Enter Tag**
   - Type tag name
   - Press Enter or click "Add" button
   - Tag appears in tag list

### Removing Tags

1. **Click Delete Button**
   - Red X icon on tag badge
   - Tag is removed immediately

## Publishing a Schema

### Publishing Workflow

1. **Prepare Schema**
   - Define all fields
   - Configure validations (optional)
   - Configure transformations (optional)
   - Define business keys (Master schemas)

2. **Validate Schema**
   - Click "Publish" button
   - System runs validation
   - Button color indicates status:
     - Green: Ready to publish
     - Red: Has errors

3. **Review Errors**
   - If errors exist, review error messages
   - Errors are field-addressable
   - Navigate to relevant tabs to fix errors

4. **Handle Dependencies**
   - If unpublished dependencies exist, modal appears
   - Modal lists all unpublished schemas
   - Option to bulk publish dependencies
   - Confirm to publish all related schemas

5. **Publish Schema**
   - If validation passes, schema is published
   - Status changes to Published
   - Schema becomes read-only
   - Success message displayed

## Deleting Schemas

### Deleting a Schema Version

1. **Navigate to Schema Version Detail**
   - Open the version to delete (must be latest)

2. **Click "Delete Version" Button**
   - Red border button in header
   - Only visible for latest version

3. **Confirm Deletion**
   - Modal dialog appears
   - Shows version number and schema key
   - Click "Delete" to confirm
   - Version is deleted
   - User is redirected to schema key overview

### Deleting an Entire Schema

1. **Navigate to Schema Overview**
   - Go to schema overview page

2. **Click Delete Button**
   - Red X icon on schema card

3. **Confirm Deletion**
   - Modal dialog appears
   - Warns about deleting all versions
   - Click "Delete" to confirm

4. **Handle Errors**
   - If schema is referenced, error message appears
   - Error lists all referencing schemas
   - Must remove references before deletion

## Viewing Schema References

### Dependency Graph

1. **Enable Expert Mode**
   - Check "Expert Mode" checkbox

2. **Navigate to References Tab**
   - Click on References tab

3. **View Graph**
   - Visual graph showing schema dependencies
   - Nodes represent schemas
   - Edges represent references
   - Interactive graph with zoom/pan

## Filtering and Searching

### Filtering Schemas

1. **Select Status Filter**
   - Dropdown in schema overview
   - Options: All, Draft, Published, Archived
   - List updates immediately

2. **Search Schemas**
   - Search box in schema overview
   - Search by key or description
   - Results update as you type

### Schema Organization

- **Tags**: Use tags to categorize schemas
- **Status Badges**: Visual indicators for schema status
- **Role Separation**: Schemas organized by role (Incoming, Master, Outgoing)


