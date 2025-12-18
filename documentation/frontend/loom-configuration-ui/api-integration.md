# API Integration

This document describes how the frontend integrates with backend services.

## Service Architecture

The frontend communicates with two backend services:

1. **Configuration Service**: Workflow configuration and management
2. **Layout Service**: Node position persistence

Both services are accessed through a single API gateway (Nginx proxy).

## API Client

### Base Client (`api/client.ts`)

**Purpose**: Wraps native Fetch API with common functionality

**Features**:
- Base URL configuration via environment variables
- Automatic JSON serialization/deserialization
- Error handling
- Content-Type headers

**Implementation**:
```typescript
const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api'

async function request<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${endpoint}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  })
  
  if (!response.ok) {
    throw new ApiError(response.status, error || response.statusText)
  }
  
  return response.json()
}
```

## Configuration Service API

### Workflow Operations

**Get Workflows**:
- `GET /workflows?tenantId={tenantId}`
- Returns: Array of workflow definitions

**Create Workflow**:
- `POST /workflows`
- Body: `{ tenantId, name, description? }`
- Returns: `{ id: string }`

**Get Versions**:
- `GET /workflows/{workflowId}/versions`
- Returns: Array of workflow versions

**Get Version Details**:
- `GET /workflows/versions/{versionId}`
- Returns: Complete workflow version details

**Create Draft Version**:
- `POST /workflows/{workflowId}/versions/draft`
- Body: `{ createdBy }`
- Returns: `{ id: string }`

**Publish Version**:
- `POST /workflows/versions/{versionId}/publish`
- Body: `{ publishedBy }`
- Returns: `{ success: boolean }`

**Delete Version**:
- `DELETE /workflows/versions/{versionId}`
- Returns: `{ success: boolean }`

**Validate Version**:
- `GET /workflows/versions/{versionId}/validate`
- Returns: Validation result with errors and warnings

### Node Operations

**Add Node**:
- `POST /nodes`
- Body: `{ workflowVersionId, key, name, type, config? }`
- Returns: `{ id: string }`

**Update Node Metadata**:
- `PUT /nodes/{nodeId}`
- Body: `{ nodeId, name?, type? }`
- Returns: `{ success: boolean }`

**Update Node Config**:
- `PUT /nodes/{nodeId}/config`
- Body: `{ config }`
- Returns: `{ success: boolean }`

**Remove Node**:
- `DELETE /nodes/{nodeId}`
- Returns: `{ success: boolean }`

### Connection Operations

**Add Connection**:
- `POST /connections`
- Body: `{ workflowVersionId, fromNodeId, toNodeId, connectionType, order? }`
- Returns: `{ id: string }`

**Remove Connection**:
- `DELETE /connections/{connectionId}`
- Returns: `{ success: boolean }`

### Trigger Operations

**Create Trigger**:
- `POST /triggers`
- Body: `{ tenantId, type, config? }`
- Returns: `{ id: string }`

**Bind Trigger to Workflow**:
- `POST /triggers/bind`
- Body: `{ triggerId, workflowVersionId, priority, enabled }`
- Returns: `{ id: string }`

**Unbind Trigger**:
- `DELETE /triggers/bindings/{bindingId}`
- Returns: `{ success: boolean }`

**Bind Trigger to Node**:
- `POST /triggers/bindings/nodes`
- Body: `{ triggerBindingId, entryNodeId, order? }`
- Returns: `{ id: string }`

**Unbind Trigger from Node**:
- `DELETE /triggers/bindings/nodes/{nodeBindingId}`
- Returns: `{ success: boolean }`

### Variable Operations

**Add Variable**:
- `POST /variables`
- Body: `{ workflowVersionId, key, type, initialValue?, description? }`
- Returns: `{ id: string }`

**Update Variable**:
- `PUT /variables/{variableId}`
- Body: `{ type, initialValue?, description? }`
- Returns: `{ success: boolean }`

**Remove Variable**:
- `DELETE /variables/{variableId}`
- Returns: `{ success: boolean }`

### Label Operations

**Add Label**:
- `POST /labels`
- Body: `{ workflowVersionId, key, type, description? }`
- Returns: `{ id: string }`

**Remove Label**:
- `DELETE /labels/{labelId}`
- Returns: `{ success: boolean }`

## Layout Service API

### Layout Operations

**Get Layout**:
- `GET /workflow-versions/{workflowVersionId}/layout`
- Returns: `{ nodes: Array<{ nodeKey, x, y, width?, height? }> }`

**Upsert Node Layout**:
- `PUT /workflow-versions/{workflowVersionId}/layout/nodes/{nodeKey}`
- Body: `{ x, y, width?, height? }`
- Returns: `{ success: boolean }`

**Upsert Node Layouts Batch**:
- `PUT /workflow-versions/{workflowVersionId}/layout/nodes`
- Body: `{ nodes: Array<{ nodeKey, x, y, width?, height? }> }`
- Returns: `{ success: boolean }`

**Copy Layout from Version**:
- `POST /workflow-versions/{targetVersionId}/layout/copy-from/{sourceVersionId}`
- Returns: `{ success: boolean }`

**Delete Node Layout**:
- `DELETE /workflow-versions/{workflowVersionId}/layout/nodes/{nodeKey}`
- Returns: `{ success: boolean }`

## React Query Integration

### Query Hooks

**Workflow List**:
```typescript
useQuery({
  queryKey: ['workflows'],
  queryFn: workflowsApi.getWorkflows,
})
```

**Workflow Versions**:
```typescript
useQuery({
  queryKey: ['versions', workflowId],
  queryFn: () => workflowsApi.getVersions(workflowId),
  enabled: !!workflowId,
})
```

**Version Details**:
```typescript
useQuery({
  queryKey: ['versionDetails', versionId],
  queryFn: () => workflowsApi.getVersionDetails(versionId),
  enabled: !!versionId,
})
```

**Layout Data**:
```typescript
useQuery({
  queryKey: ['layout', versionId],
  queryFn: () => layoutApi.getLayout(versionId),
  enabled: !!versionId,
})
```

### Mutation Hooks

**Create Workflow**:
```typescript
useMutation({
  mutationFn: () => workflowsApi.createWorkflow(name, description),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['workflows'] })
  },
})
```

**Add Node**:
```typescript
useMutation({
  mutationFn: (data) => nodesApi.addNode(versionId, data.key, data.name, data.type),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['versionDetails', versionId] })
  },
})
```

**Save Layout**:
```typescript
useMutation({
  mutationFn: (layouts) => layoutApi.upsertNodeLayoutsBatch(versionId, layouts),
  // No query invalidation needed (layout is separate)
})
```

## Error Handling

### API Errors

**Network Errors**:
- Displayed in UI with error message
- User can retry operation

**Validation Errors**:
- Shown inline in relevant panels
- Blocking errors prevent publishing

**404 Errors**:
- Handled gracefully
- Redirect or show not found message

### Error Display

- **Toast Notifications**: For operation feedback (future)
- **Inline Errors**: In form fields and panels
- **Error Boundaries**: Catch React errors (future)

## Request/Response Flow

### Typical Flow

1. **User Action**: User performs action (e.g., add node)
2. **Mutation Triggered**: React Query mutation called
3. **API Request**: HTTP request sent to backend
4. **Response Handling**: Success or error response
5. **Query Invalidation**: Related queries invalidated
6. **UI Update**: UI updates with new data

### Optimistic Updates

- **Immediate UI Update**: UI updates before API response
- **Rollback on Error**: Reverts if API call fails
- **Used For**: Non-critical operations (future enhancement)

## Environment Configuration

### Development

- **Local Backend**: `http://localhost:8080/api`
- **Local Layout**: `http://localhost:8081/api/layout`
- **Nginx Proxy**: Routes `/api` to backend services

### Production

- **Kubernetes Services**: Service names for backend
- **Nginx Proxy**: Routes to Kubernetes services
- **Environment Variables**: Set at build time

### Environment Variables

- `VITE_API_BASE_URL`: Configuration service base URL
- `VITE_LAYOUT_API_URL`: Layout service base URL
- Defaults: `/api` (proxied through Nginx)

