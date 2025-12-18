# API Overview

The Layout Service exposes a **HTTP REST API** only. All endpoints are scoped to a workflow version.

## Base Path

All endpoints are under: `/api/workflow-versions/{workflowVersionId}/layout`

## Authentication & Tenant Isolation

- **Tenant ID**: Extracted from `X-Tenant-Id` HTTP header
- **Default Tenant**: If header is missing, default tenant ID is used (for development)
- **Isolation**: All operations are filtered by tenant ID

## Endpoints

### Get Layout for Workflow Version

- **GET** `/api/workflow-versions/{workflowVersionId}/layout`
- Retrieves all node layouts for a workflow version
- **Parameters**:
  - `workflowVersionId` (path): Workflow version UUID
- **Response**: `WorkflowVersionLayoutResponse`
  ```json
  {
    "nodes": [
      {
        "nodeKey": "node-1",
        "x": 100.0,
        "y": 150.0,
        "width": 200.0,
        "height": 100.0
      }
    ]
  }
  ```
- **Empty Response**: Returns empty array if no layouts exist

### Get Single Node Layout

- **GET** `/api/workflow-versions/{workflowVersionId}/layout/nodes/{nodeKey}`
- Retrieves layout for a specific node
- **Parameters**:
  - `workflowVersionId` (path): Workflow version UUID
  - `nodeKey` (path): Node key (string)
- **Response**: `NodeLayoutResponse` or `404 Not Found`
  ```json
  {
    "nodeKey": "node-1",
    "x": 100.0,
    "y": 150.0,
    "width": 200.0,
    "height": 100.0
  }
  ```

### Upsert Node Layout (Single)

- **PUT** `/api/workflow-versions/{workflowVersionId}/layout/nodes/{nodeKey}`
- Creates or updates layout for a single node
- **Parameters**:
  - `workflowVersionId` (path): Workflow version UUID
  - `nodeKey` (path): Node key (string)
- **Request Body**: `UpsertNodeLayoutRequest`
  ```json
  {
    "x": 100.0,
    "y": 150.0,
    "width": 200.0,
    "height": 100.0
  }
  ```
- **Response**: `SuccessResponse`
  ```json
  {
    "success": true
  }
  ```
- **Behavior**: Idempotent (create if missing, update if exists)

### Upsert Node Layouts (Batch)

- **PUT** `/api/workflow-versions/{workflowVersionId}/layout/nodes`
- Creates or updates layouts for multiple nodes in a single transaction
- **Parameters**:
  - `workflowVersionId` (path): Workflow version UUID
- **Request Body**: `UpsertNodeLayoutsBatchRequest`
  ```json
  {
    "nodes": [
      {
        "nodeKey": "node-1",
        "x": 100.0,
        "y": 150.0,
        "width": 200.0,
        "height": 100.0
      },
      {
        "nodeKey": "node-2",
        "x": 350.0,
        "y": 150.0
      }
    ]
  }
  ```
- **Response**: `SuccessResponse`
- **Behavior**: 
  - All nodes are updated atomically (single transaction)
  - Idempotent (create if missing, update if exists)
  - More efficient than multiple single-node requests

### Copy Layout from Workflow Version

- **POST** `/api/workflow-versions/{workflowVersionId}/layout/copy-from/{sourceWorkflowVersionId}`
- Copies all node layouts from source version to target version
- **Parameters**:
  - `workflowVersionId` (path): Target workflow version UUID
  - `sourceWorkflowVersionId` (path): Source workflow version UUID
- **Response**: `SuccessResponse`
- **Behavior**:
  - Copies all node layouts from source to target
  - Uses node keys to match layouts
  - Overwrites existing layouts in target
  - Idempotent (safe to call multiple times)

### Delete Node Layout

- **DELETE** `/api/workflow-versions/{workflowVersionId}/layout/nodes/{nodeKey}`
- Deletes layout for a specific node
- **Parameters**:
  - `workflowVersionId` (path): Workflow version UUID
  - `nodeKey` (path): Node key (string)
- **Response**: `SuccessResponse`
- **Behavior**: 
  - Removes the node layout
  - Does not affect other node layouts
  - Does not affect workflow version layout metadata

## Response Formats

### Success Response

```json
{
  "success": true
}
```

### Node Layout Response

```json
{
  "nodeKey": "node-1",
  "x": 100.0,
  "y": 150.0,
  "width": 200.0,
  "height": 100.0
}
```

### Workflow Version Layout Response

```json
{
  "nodes": [
    {
      "nodeKey": "node-1",
      "x": 100.0,
      "y": 150.0,
      "width": 200.0,
      "height": 100.0
    }
  ]
}
```

## Error Responses

- **400 Bad Request**: Invalid input parameters
- **404 Not Found**: Resource not found (for single node layout queries)
- **500 Internal Server Error**: Unexpected server error

## Best Practices

### Saving Layouts

1. **Debounce Saves**: Implement debouncing (e.g., 500ms) when dragging nodes
2. **Batch Updates**: Use batch endpoint when updating multiple nodes
3. **Final Save**: Always save on drag stop to ensure positions are persisted

### Loading Layouts

1. **Load on Open**: Load layouts when opening a workflow version
2. **Handle Missing**: Gracefully handle missing layouts with default positions
3. **Cache**: Consider caching layouts to reduce API calls

### Copying Layouts

1. **After Draft Creation**: Copy layout after creating draft from published version
2. **User Choice**: Consider making layout copy optional (user preference)
3. **Error Handling**: Handle copy failures gracefully (fallback to default positions)

