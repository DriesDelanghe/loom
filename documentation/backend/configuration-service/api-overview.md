# API Overview

The Configuration Service exposes two APIs: HTTP REST and gRPC. Both provide the same functionality.

## HTTP REST API

Base path: `/api`

### Workflow Endpoints

#### Create Workflow Definition
- **POST** `/workflows`
- Creates a new workflow definition
- Returns: Workflow definition ID

#### Get Workflow Definitions
- **GET** `/workflows?tenantId={tenantId}`
- Lists all workflow definitions for a tenant
- Returns: Array of workflow definitions

#### Get Workflow Versions
- **GET** `/workflows/{workflowId}/versions`
- Lists all versions of a workflow
- Returns: Array of workflow versions

#### Get Workflow Version Details
- **GET** `/workflows/versions/{versionId}`
- Gets complete details of a workflow version
- Returns: Version with nodes, connections, triggers, variables, labels, settings

#### Create Draft Workflow Version
- **POST** `/workflows/{workflowId}/versions/draft`
- Creates a new draft version (optionally from a published version)
- Returns: New version ID

#### Publish Workflow Version
- **POST** `/workflows/versions/{versionId}/publish`
- Publishes a draft version (makes it immutable and executable)
- Returns: Success status

#### Delete Workflow Version
- **DELETE** `/workflows/versions/{versionId}`
- Deletes a draft version (only Draft versions can be deleted)
- Returns: Success status

#### Validate Workflow Version
- **GET** `/workflows/versions/{versionId}/validate`
- Validates a workflow version
- Returns: Validation result with errors and warnings

#### Get Compiled Workflow
- **GET** `/workflows/versions/{versionId}/compiled`
- Gets the compiled (execution-ready) representation of a published version
- Returns: Compiled workflow structure

#### Archive Workflow Definition
- **POST** `/workflows/{workflowId}/archive`
- Archives a workflow definition
- Returns: Success status

### Node Endpoints

#### Add Node
- **POST** `/nodes`
- Adds a node to a workflow version
- Returns: Node ID

#### Update Node Metadata
- **PUT** `/nodes/{nodeId}`
- Updates node name and/or type
- Returns: Success status

#### Update Node Config
- **PUT** `/nodes/{nodeId}/config`
- Updates node configuration
- Returns: Success status

#### Remove Node
- **DELETE** `/nodes/{nodeId}`
- Removes a node from a workflow version
- Returns: Success status

### Connection Endpoints

#### Add Connection
- **POST** `/connections`
- Adds a connection between two nodes
- Request body includes: `workflowVersionId`, `fromNodeId`, `toNodeId`, `outcome` (semantic outcome string), `order` (optional)
- Outcome is determined by connector position in UI (top = positive, bottom = negative)
- Returns: Connection ID

#### Remove Connection
- **DELETE** `/connections/{connectionId}`
- Removes a connection
- Returns: Success status

### Trigger Endpoints

#### Create Trigger
- **POST** `/triggers`
- Creates a new trigger
- Returns: Trigger ID

#### Update Trigger Config
- **PUT** `/triggers/{triggerId}`
- Updates trigger configuration
- Returns: Success status

#### Delete Trigger
- **DELETE** `/triggers/{triggerId}`
- Deletes a trigger
- Returns: Success status

#### Bind Trigger to Workflow Version
- **POST** `/triggers/bind`
- Binds a trigger to a workflow version
- Returns: Trigger binding ID

#### Unbind Trigger from Workflow Version
- **DELETE** `/triggers/bindings/{bindingId}`
- Unbinds a trigger from a workflow version
- Returns: Success status

#### Bind Trigger to Node
- **POST** `/triggers/bindings/nodes`
- Binds a trigger to an entry node
- Returns: Trigger node binding ID

#### Unbind Trigger from Node
- **DELETE** `/triggers/bindings/nodes/{nodeBindingId}`
- Unbinds a trigger from an entry node
- Returns: Success status

#### Get Workflow Versions for Trigger
- **GET** `/triggers/{triggerId}/workflow-versions`
- Lists all workflow versions bound to a trigger
- Returns: Array of workflow versions

### Variable Endpoints

#### Add Variable
- **POST** `/variables`
- Adds a variable to a workflow version
- Returns: Variable ID

#### Update Variable
- **PUT** `/variables/{variableId}`
- Updates a variable
- Returns: Success status

#### Remove Variable
- **DELETE** `/variables/{variableId}`
- Removes a variable
- Returns: Success status

### Label Endpoints

#### Add Label
- **POST** `/labels`
- Adds a label to a workflow version
- Returns: Label ID

#### Remove Label
- **DELETE** `/labels/{labelId}`
- Removes a label
- Returns: Success status

## gRPC API

The gRPC API provides the same operations as the HTTP REST API but uses Protocol Buffers for serialization.

### Service Interface

`IConfigurationService` exposes all operations as gRPC methods.

### Benefits

- **Performance**: More efficient binary serialization
- **Streaming**: Support for streaming operations (future)
- **Type Safety**: Strong typing with Protocol Buffers
- **Inter-Service Communication**: Optimized for service-to-service calls

### Usage

The gRPC API is primarily intended for:
- Service-to-service communication
- High-performance scenarios
- Real-time operations

For web clients, the HTTP REST API is typically more convenient.

## Response Formats

### Success Responses

- **IdResponse**: Contains an ID (string UUID)
- **SuccessResponse**: Contains a success boolean

### Error Responses

- **400 Bad Request**: Invalid input parameters
- **404 Not Found**: Resource not found
- **409 Conflict**: Business rule violation (e.g., duplicate connection)
- **500 Internal Server Error**: Unexpected server error

### Validation Responses

- **ValidationResult**: Contains isValid boolean, errors array, warnings array

## Authentication & Authorization

Currently, the service does not implement authentication/authorization. This is planned for future implementation.

## Rate Limiting

Currently, the service does not implement rate limiting. This is planned for future implementation.

